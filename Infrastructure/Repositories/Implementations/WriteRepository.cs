using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Infrastructure.DataContext;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Infrastructure.Repositories.Implementations;

public class WriteRepository<T>
    : IWriteRepository<T>
    where T : class, IAggregateRoot
{
    private readonly ApplicationDbContext _context;

    public WriteRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        await _context
            .Set<T>()
            .AddAsync(
                entity,
                cancellationToken);
    }

    public IQueryable<T> Query()
    {
        return _context
            .Set<T>();
    }

    // no async because we are using asyn operation findasync 
    public  ValueTask<T?> FindAsync(
        object[] keyValues,
        CancellationToken cancellationToken = default)
    {
      return  _context
            .Set<T>()
            .FindAsync(
                keyValues,
                cancellationToken);
    }

    public void Update(T entity)
    {
        _context
            .Set<T>()
            .Update(entity);
    }

    public void Remove(T entity)
    {
        _context
            .Set<T>()
            .Remove(entity);
    }

    public async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        await _context
            .Set<T>()
            .AddRangeAsync(
                entities,
                cancellationToken);
    }

    public void RemoveRange(
        IEnumerable<T> entities)
    {
        _context
            .Set<T>()
            .RemoveRange(entities);
    }
}