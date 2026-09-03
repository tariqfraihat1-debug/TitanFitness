using System.Linq.Expressions;
using CSharpFunctionalExtensions;

namespace TitanFitness.Infrastructure.Repositories.Interfaces;

public interface IReadOnlyRepository<T, TId>
    where T : class
    where TId : notnull
{
    IQueryable<T> GetAll();

    Task<Maybe<T>> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default);

    Task<Maybe<T>> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);


}