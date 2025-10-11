using System;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Plans;

namespace SDV.Domain.Interfaces.Plans;

public interface IPlanService
{
    Task<Result<IEnumerable<Plan>>> GetAllPlansAsync();
    Task<Result<Plan>> GetPlanByIdAsync(Guid id);
    Task<Result<Plan>> CreatePlanAsync(Plan plan);
    Task<Result<Plan>> UpdatePlanAsync(Plan plan);
    Task<Result<bool>> DeactivatePlanAsync(Guid id);

}
