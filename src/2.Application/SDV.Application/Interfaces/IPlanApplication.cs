using SDV.Application.Dtos.Plans;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

public interface IPlanApplication
{
    Task<OperationResult<IEnumerable<PlanDto>>> GetAllPlans();
    Task<OperationResult<PlanDto>> GetPlanById(string id);
    Task<OperationResult<PlanDto>> CreatePlan(PlanDto dto);
    Task<OperationResult<PlanDto>> UpdatePlan(string id, PlanDto dto);
    Task<OperationResult<bool>> DeactivatePlan(string id);
}