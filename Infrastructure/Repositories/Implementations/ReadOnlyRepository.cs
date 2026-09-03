using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Infrastructure.DataContext;
using TitanFitness.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace TitanFitness.Infrastructure.Repositories.Implementations;

public class ReadOnlyRepository<T, TId>
    : IReadOnlyRepository<T, TId>
    where T : class
    where TId : notnull
{
    private readonly ApplicationDbContext _context;

    public ReadOnlyRepository(
        ApplicationDbContext context)
    {
        _context = context;
      //  _context.ChangeTracker.AutoDetectChangesEnabled = false; i removed it beacuse its making the write nonwork in one of my handlers 
    }

    public IQueryable<T> GetAll()
    {
        return _context
            .Set<T>()
            .AsNoTracking();
    }

    public async Task<Maybe<T>> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        T? entity = await _context
            .Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity =>
                    EF.Property<TId>(entity, "Id")
                        .Equals(id),
                cancellationToken);

        return entity;
    }

    public async Task<Maybe<T>> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        T? entity = await _context
            .Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                predicate,
                cancellationToken);

        return entity;
    }

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context
            .Set<T>()
            .AsNoTracking()
            .AnyAsync(
                predicate,
                cancellationToken);
    }

}