using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AssetControl.Api.Data;
using AssetControl.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;
namespace AssetControl.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;

    public AuthController(AppDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    public record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Email e senha são obrigatórios." });

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null) return Unauthorized(new { error = "Credenciais inválidas." });

        var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!ok) return Unauthorized(new { error = "Credenciais inválidas." });

        var token = CreateJwt(user);
        return Ok(new
        {
            token,
            name = user.Name,
            email = user.Email,
            role = user.Role
        });
    }

    // Endpoint para CRIAR o admin a partir de variáveis de ambiente/appsettings
    // Use uma vez para popular o banco do container.
    [HttpPost("bootstrap")]
    [AllowAnonymous]
    public async Task<IActionResult> Bootstrap()
    {
        var email = _cfg["Seed:AdminEmail"] ?? "admin@local";
        var password = _cfg["Seed:AdminPassword"] ?? "Admin@123";

        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Ok(new { created = false, note = "Usuário já existe." });

        var hash = BCrypt.Net.BCrypt.HashPassword(password);
         // Adicione esta linha no início do arquivo para corrigir o erro CS0117.
        _db.Users.Add(new User
        {
            Name = "Admin",
            Email = email,
            PasswordHash = hash,
            Role = "Admin"
        });
        await _db.SaveChangesAsync();

        return Ok(new { created = true, email });
    }

    private string CreateJwt(User user)
    {
        var key = _cfg["Auth:Jwt:Key"] ?? "CHANGE_THIS_SUPER_SECRET_KEY_32_CHARS_MIN";
        var issuer = _cfg["Auth:Jwt:Issuer"] ?? "AssetControl";
        var audience = _cfg["Auth:Jwt:Audience"] ?? "AssetControl";

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
