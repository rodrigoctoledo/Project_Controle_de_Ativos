using AssetControl.Api.Data;
using AssetControl.Application.DTOs;
using AssetControl.Application.Services;
using AssetControl.Domain;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AssetControl.Tests;

public class AssetServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly AssetService _service;

    public AssetServiceTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conn)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _service = new AssetService(_db);
    }

    [Fact]
    public async Task Create_Should_Insert_And_EnforceUniqueCode()
    {
        var a1 = await _service.CreateAsync(new AssetCreateDto { Name = "Monitor", Code = "MON-1" });
        a1.Id.Should().BeGreaterThan(0);

        var act = async () => await _service.CreateAsync(new AssetCreateDto { Name = "Outro", Code = "MON-1" });
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Código já cadastrado*");
    }

    [Fact]
    public async Task Checkout_Then_Checkin_Should_Change_Status_And_Fields()
    {
        var a = await _service.CreateAsync(new AssetCreateDto { Name = "Teclado", Code = "TEC-1" });
        a.Status.Should().Be(AssetStatus.Available);

        var afterCheckout = await _service.CheckoutAsync(a.Id, new CheckoutDto { TakenBy = "Rodrigo", Note = "Mesa 2" });
        afterCheckout!.Status.Should().Be(AssetStatus.InUse);
        afterCheckout.CheckedOutBy.Should().Be("Rodrigo");
        afterCheckout.Notes.Should().Be("Mesa 2");

        var afterCheckin = await _service.CheckinAsync(a.Id);
        afterCheckin!.Status.Should().Be(AssetStatus.Available);
        afterCheckin.CheckedOutBy.Should().BeNull();
        afterCheckin.Notes.Should().BeNull();
    }

    [Fact]
    public async Task List_With_Search_Sort_And_Pagination_Should_Work()
    {
        await _service.CreateAsync(new AssetCreateDto { Name = "Monitor Dell", Code = "A" });
        await _service.CreateAsync(new AssetCreateDto { Name = "Monitor Samsung", Code = "B" });
        await _service.CreateAsync(new AssetCreateDto { Name = "Projetor Epson", Code = "C" });

        var page1 = await _service.ListAsync(new AssetControl.Application.DTOs.AssetQueryParams { Page = 1, PageSize = 2, SortBy = "name", SortDir = "asc" });
        page1.Total.Should().Be(3);
        page1.Items.Should().HaveCount(2);

        var search = await _service.ListAsync(new AssetControl.Application.DTOs.AssetQueryParams { Search = "monitor", Page = 1, PageSize = 10 });
        search.Total.Should().Be(2);
        search.Items.Should().OnlyContain(a => a.Name.ToLower().Contains("monitor"));
    }

    [Fact]
    public async Task Delete_Should_Remove()
    {
        var a = await _service.CreateAsync(new AssetCreateDto { Name = "Proj", Code = "X" });
        var ok = await _service.DeleteAsync(a.Id);
        ok.Should().BeTrue();

        var get = await _service.GetAsync(a.Id);
        get.Should().BeNull();
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }
}
