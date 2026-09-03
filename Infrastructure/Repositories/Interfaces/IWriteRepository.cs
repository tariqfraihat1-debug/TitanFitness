using TitanFitness.Domain.Common.Entities;

namespace TitanFitness.Infrastructure.Repositories.Interfaces;

public interface IWriteRepository<T>
    where T : class, IAggregateRoot
{
    Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default);

    IQueryable<T> Query();

    ValueTask<T?> FindAsync(
        object[] keyValues,
        CancellationToken cancellationToken = default);

    void Update(
        T entity);

    void Remove(
        T entity);

    Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default);

    void RemoveRange(
        IEnumerable<T> entities);
}