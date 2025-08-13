using System;
using System.IO;
using System.Text;
using AssetControl.Api.Data;
using AssetControl.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Expõe a API na porta 5000 (dentro e fora do container)
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// =============== CORS ===============
// Opção 1: ABERTO SEM CREDENCIAIS (recomendado para testes / JWT via header)
builder.Services.AddCors(o =>
{
    o.AddPolicy("Open", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

/*
// Opção 2: ABERTO COM CREDENCIAIS (cookies, fetch com credentials)
// NÃO use AllowAnyOrigin() junto com AllowCredentials().
// Use SetIsOriginAllowed(_ => true) para ecoar a origem dinamicamente.
builder.Services.AddCors(o =>
{
    o.AddPolicy("OpenCreds", p => p
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});
*/

// =============== MVC/Controllers ===============
builder.Services.AddControllers();

// =============== Swagger ===============
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AssetControl API",
        Version = "v1",
        Description = "API do AssetControl"
    });

    // (Opcional) Suporte a JWT na UI do Swagger
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Insira: Bearer {seu_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// =============== DB/EF Core ===============
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=/app/data/app.db"));

// Caso use a abstração IAppDbContext em Services/Handlers:
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// =============== (Opcional) Autenticação JWT ===============
/*
var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrWhiteSpace(jwtKey))
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key
            };
        });
}
*/

var app = builder.Build();

// Garante pasta do banco (bom para rodar em container)
Directory.CreateDirectory("/app/data");

// =============== Middleware Order ===============
app.UseCors("Open");             // ou "OpenCreds" se for usar cookies
// app.UseAuthentication();      // se habilitar JWT acima
// app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AssetControl API v1");
});

// Redireciona raiz para o Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Aplica migrations no start
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Controllers
app.MapControllers();

app.Run();
