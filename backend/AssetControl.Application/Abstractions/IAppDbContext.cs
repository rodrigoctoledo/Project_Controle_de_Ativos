using AssetControl.Domain;
using AssetControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AssetControl.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Asset> Assets { get; }
    DbSet<User> Users { get; }  // ADICIONAR ESTA LINHA
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
