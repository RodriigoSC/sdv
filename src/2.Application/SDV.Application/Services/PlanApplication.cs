using SDV.Application.Dtos.Plans;
using SDV.Application.Interfaces;
using SDV.Application.Mappers;
using SDV.Application.Results;
using SDV.Domain.Interfaces.Plans;

namespace SDV.Application.Services;

public class PlanApplication : IPlanApplication
{
    private readonly IPlanService _planService;

    public PlanApplication(IPlanService planService)
    {
        _planService = planService;
    }

    public async Task<OperationResult<IEnumerable<PlanDto>>> GetAllPlans()
    {
        var result = await _planService.GetAllPlansAsync();
        if (!result.IsSuccess)
            return OperationResult<IEnumerable<PlanDto>>.Failed(null, result.Error ?? "Planos não encontrados.", 404);

        return OperationResult<IEnumerable<PlanDto>>.Succeeded(result.Value!.ToPlanDtoList(), "Planos encontrados.");
    }

    public async Task<OperationResult<PlanDto>> GetPlanById(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<PlanDto>.Failed(null, "ID de plano inválido.", 400);

        var result = await _planService.GetPlanByIdAsync(guid);

        if (!result.IsSuccess)
            return OperationResult<PlanDto>.Failed(null, result.Error ?? "Plano não encontrado.", 404);

        return OperationResult<PlanDto>.Succeeded(result.Value!.ToPlanDto(), "Plano encontrado.");
    }

    public async Task<OperationResult<PlanDto>> CreatePlan(PlanDto dto)
    {
        try
        {
            var plan = dto.ToPlan();
            var result = await _planService.CreatePlanAsync(plan);

            if (!result.IsSuccess)
                return OperationResult<PlanDto>.Failed(null, result.Error ?? "Plano não criado.", 406);

            return OperationResult<PlanDto>.Succeeded(result.Value!.ToPlanDto(), "Plano criado com sucesso.", 201);
        }
        catch (Exception ex)
        {
            return OperationResult<PlanDto>.Failed(null, ex.Message, 400);
        }
    }

    public async Task<OperationResult<PlanDto>> UpdatePlan(string id, PlanDto dto)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<PlanDto>.Failed(null, "ID de plano inválido.", 400);
        
        var plan = dto.ToPlan();
        var result = await _planService.UpdatePlanAsync(plan);

        if (!result.IsSuccess)
            return OperationResult<PlanDto>.Failed(null, result.Error ?? "Plano não atualizado.", 400);

        return OperationResult<PlanDto>.Succeeded(result.Value!.ToPlanDto(), "Plano atualizado com sucesso.");
    }

    public async Task<OperationResult<bool>> DeactivatePlan(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<bool>.Failed(false, "ID de plano inválido.", 400);

        var result = await _planService.DeactivatePlanAsync(guid);

        if (!result.IsSuccess)
            return OperationResult<bool>.Failed(false, result.Error ?? "Plano não desativado.", 400);

        return OperationResult<bool>.Succeeded(true, "Plano desativado com sucesso.");
    }
}