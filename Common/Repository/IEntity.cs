namespace Common.Repository;

// Base contract for entities with integer IDs
public interface IEntity
{
    int Id { get; }
}