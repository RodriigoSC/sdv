using SDV.Application.Dtos.Planners;
using SDV.Application.Interfaces;
using SDV.Application.Mappers;
using SDV.Application.Results;
using SDV.Domain.Interfaces.Planners;

namespace SDV.Application.Services;

public class PlannerApplication : IPlannerApplication
{
    private readonly IPlannerService _plannerService;

    public PlannerApplication(IPlannerService plannerService)
    {
        _plannerService = plannerService;
    }

    #region Consultas

    public async Task<OperationResult<IEnumerable<PlannerDto>>> GetAllPlanners(string clientId)
    {
        if (!Guid.TryParse(clientId, out var clientGuid))
            return OperationResult<IEnumerable<PlannerDto>>.Failed(null, "Invalid client ID format", 400);

        var result = await _plannerService.GetAllPlannersAsync(clientGuid);
        if (!result.IsSuccess)
            return OperationResult<IEnumerable<PlannerDto>>.Failed(null, result.Error ?? "Planners not retrieved", 400);

        var dtos = result.Value?.Select(a => a.ToPlannerDto()) ?? Enumerable.Empty<PlannerDto>();
        return OperationResult<IEnumerable<PlannerDto>>.Succeeded(dtos, "Planners retrieved", 200);
    }

    public async Task<OperationResult<PlannerDto>> GetPlannerById(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<PlannerDto>.Failed(null, "Invalid ID format", 400);

        var result = await _plannerService.GetPlannerByIdAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<PlannerDto>.Failed(null, result.Error ?? "Planner not found", 404);

        return OperationResult<PlannerDto>.Succeeded(result.Value!.ToPlannerDto(), "Planner retrieved", 200);
    }

    #endregion

    #region Criação

    public async Task<OperationResult<PlannerDto>> CreatePlanner(PlannerDto dto)
    {
        try
        {
            var planner = dto.ToPlanner();
            var result = await _plannerService.CreatePlannerAsync(planner);

            if (!result.IsSuccess)
                return OperationResult<PlannerDto>.Failed(null, result.Error ?? "Planner not created", 406);

            return OperationResult<PlannerDto>.Succeeded(planner.ToPlannerDto(), "Planner created", 201);
        }
        catch (Exception ex)
        {
            return OperationResult<PlannerDto>.Failed(null, ex.Message, 400);
        }
    }

    #endregion

    #region Atualizações

    public async Task<OperationResult<PlannerDto>> UpdatePlanner(string id, PlannerDto dto)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<PlannerDto>.Failed(null, "Invalid ID format", 400);

        var existingResult = await _plannerService.GetPlannerByIdAsync(guid);
        if (!existingResult.IsSuccess)
            return OperationResult<PlannerDto>.Failed(null, existingResult.Error!, 404);

        var planner = existingResult.Value!;
        planner.UpdateFromDto(dto);

        var updateResult = await _plannerService.UpdatePlannerAsync(planner);
        if (!updateResult.IsSuccess)
            return OperationResult<PlannerDto>.Failed(null, updateResult.Error ?? "Planner not updated", 400);

        return OperationResult<PlannerDto>.Succeeded(planner.ToPlannerDto(), "Planner updated", 200);
    }

    public async Task<OperationResult<PlannerDto>> UpdatePlannerTitle(string id, string newTitle)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<PlannerDto>.Failed(null, "Invalid ID format", 400);

        var result = await _plannerService.UpdatePlannerTitleAsync(guid, newTitle);
        if (!result.IsSuccess)
            return OperationResult<PlannerDto>.Failed(null, result.Error ?? "Title not updated", 400);

        return OperationResult<PlannerDto>.Succeeded(result.Value!.ToPlannerDto(), "Title updated", 200);
    }

    public async Task<OperationResult<PlannerDto>> UpdatePlannerConfiguration(string id, PlannerConfigurationDto dto)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<PlannerDto>.Failed(null, "Invalid ID format", 400);

        var newConfig = dto.ToConfigurationEntity();
        var result = await _plannerService.UpdatePlannerConfigurationAsync(guid, newConfig);

        if (!result.IsSuccess)
            return OperationResult<PlannerDto>.Failed(null, result.Error ?? "Configuration not updated", 400);

        return OperationResult<PlannerDto>.Succeeded(result.Value!.ToPlannerDto(), "Configuration updated", 200);
    }

    public async Task<OperationResult<PlannerDto>> UpdatePlannerSeason(string id, PlannerSeasonDto dto)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<PlannerDto>.Failed(null, "Invalid ID format", 400);

        var newSeason = dto.ToSeasonEntity();
        var result = await _plannerService.UpdatePlannerSeasonAsync(guid, newSeason);

        if (!result.IsSuccess)
            return OperationResult<PlannerDto>.Failed(null, result.Error ?? "Season not updated", 400);

        return OperationResult<PlannerDto>.Succeeded(result.Value!.ToPlannerDto(), "Season updated", 200);
    }

    #endregion

    #region Exclusão

    public async Task<OperationResult<bool>> DeletePlanner(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<bool>.Failed(false, "Invalid ID format", 400);

        var result = await _plannerService.DeletePlannerAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<bool>.Failed(false, result.Error ?? "Planner not deleted", 400);

        return OperationResult<bool>.Succeeded(true, "Planner deleted", 200);
    }

    #endregion

    #region Geração


    public async Task<OperationResult<byte[]>> GeneratePlannerFile(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<byte[]>.Failed(null, "ID com formato inválido", 400);

        // Chama o serviço para obter o resultado
        var serviceResult = await _plannerService.GeneratePlannerFileAsync(guid);

        // Se a operação no serviço falhou...
        if (!serviceResult.IsSuccess)
        {
            return OperationResult<byte[]>.Failed(null, serviceResult.Error ?? "Ocorreu um erro desconhecido.", 404);         }

        // Se a operação foi um sucesso
        return OperationResult<byte[]>.Succeeded(serviceResult.Value, "Arquivo do Planner gerado", 200);
    }

    #endregion
}
