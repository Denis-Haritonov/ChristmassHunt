namespace Common.Repository;

public interface IRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T>? spec, CancellationToken ct = default);

    Task<int> CountAsync(ISpecification<T>? spec, CancellationToken ct = default);
    
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    Task UpdateAsync(T entity, CancellationToken ct = default);

    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    Task DeleteAsync(T entity, CancellationToken ct = default);

    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
}