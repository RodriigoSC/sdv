using System;
using SDV.Domain.Entities.Plans;

namespace SDV.Domain.Interfaces.Plans;

public interface IPlanRepository
{
    Task<IEnumerable<Plan>> GetAllAsync();
    Task<Plan?> GetByIdAsync(Guid id);
    Task AddAsync(Plan entity);
    Task UpdateAsync(Plan entity);
    Task DeleteAsync(Guid id);
}
