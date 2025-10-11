using SDV.Domain.Entities.Planners;

namespace SDV.Domain.Interfaces.Planners;

public interface IPlannerRepository
{
    Task<IEnumerable<Planner>> GetAllAsync(Guid clientId);
    Task<Planner?> GetByIdAsync(Guid id);
    Task AddAsync(Planner entity);
    Task UpdateAsync(Planner entity);
    Task DeleteAsync(Guid id);
}
