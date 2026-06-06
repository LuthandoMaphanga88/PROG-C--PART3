namespace Techmove.API.Repositories;

/// <summary>
/// Generic repository interface for data access abstraction.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Get entity by ID.
    /// </summary>
    /// <param name="id">Entity ID</param>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Add a new entity.
    /// </summary>
    /// <param name="entity">Entity to add</param>
    Task AddAsync(T entity);

    /// <summary>
    /// Update an existing entity.
    /// </summary>
    /// <param name="entity">Entity to update</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Delete an entity.
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Get a queryable dataset for advanced filtering.
    /// </summary>
    IQueryable<T> Query();

    /// <summary>
    /// Save changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
