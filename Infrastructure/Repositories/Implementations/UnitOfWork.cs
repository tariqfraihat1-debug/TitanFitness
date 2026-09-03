using Microsoft.EntityFrameworkCore.Storage;
using TitanFitness.Infrastructure.DataContext;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Infrastructure.Repositories.Implementations;

public class UnitOfWork
    : IUnitOfWork, IAsyncDisposable
{
    private readonly ApplicationDbContext _context;

    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context
            .SaveChangesAsync(
                cancellationToken);
    }

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            return;
        }

        _transaction =
            await _context
                .Database
                .BeginTransactionAsync(
                    cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(
                cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction is not null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync(
                cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }
    }
}