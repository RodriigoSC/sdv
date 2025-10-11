using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Exceptions;
using SDV.Domain.Interfaces.Plans;
using SDV.Domain.Specification;

namespace SDV.Infra.Data.Service.Plans;

public class PlanService : IPlanService
{
    private readonly IPlanRepository _planRepository;

    public PlanService(IPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<Result<IEnumerable<Plan>>> GetAllPlansAsync()
    {
        var plans = await _planRepository.GetAllAsync();
        return Result<IEnumerable<Plan>>.Success(plans);
    }

    public async Task<Result<Plan>> GetPlanByIdAsync(Guid id)
    {
        var plan = await _planRepository.GetByIdAsync(id);
        return plan is null
            ? Result<Plan>.Failure("Plano não encontrado.")
            : Result<Plan>.Success(plan);
    }

    public async Task<Result<Plan>> CreatePlanAsync(Plan plan)
    {
        try
        {
            new PlanValidationSpecification().IsValid(plan);
        }
        catch (EntityValidationException ex)
        {
            return Result<Plan>.Failure(ex.Message);
        }

        await _planRepository.AddAsync(plan);
        return Result<Plan>.Success(plan);
    }

    public async Task<Result<Plan>> UpdatePlanAsync(Plan plan)
    {
        var existingPlan = await _planRepository.GetByIdAsync(plan.Id);
        if (existingPlan == null)
            return Result<Plan>.Failure("Plano não encontrado.");

        try
        {
            new PlanValidationSpecification().IsValid(plan);
        }
        catch (EntityValidationException ex)
        {
            return Result<Plan>.Failure(ex.Message);
        }

        await _planRepository.UpdateAsync(plan);
        return Result<Plan>.Success(plan);
    }

    public async Task<Result<bool>> DeactivatePlanAsync(Guid id)
    {
        var plan = await _planRepository.GetByIdAsync(id);
        if (plan is null)
            return Result<bool>.Failure("Plano não encontrado.");

        plan.Deactivate();
        await _planRepository.UpdateAsync(plan);
        return Result<bool>.Success(true);
    }
}