using SDV.Application.Dtos.Agendas;
using SDV.Application.Interfaces;
using SDV.Application.Mappers;
using SDV.Application.Results;
using SDV.Domain.Interfaces.Agendas;

namespace SDV.Application.Services;

public class AgendaApplication : IAgendaApplication
{
    private readonly IAgendaService _agendaService;

    public AgendaApplication(IAgendaService agendaService)
    {
        _agendaService = agendaService;
    }

    #region Consultas

    public async Task<OperationResult<IEnumerable<AgendaDto>>> GetAllAgendas(string clientId)
    {
        if (!Guid.TryParse(clientId, out var clientGuid))
            return OperationResult<IEnumerable<AgendaDto>>.Failed(null, "Invalid client ID format", 400);

        var result = await _agendaService.GetAllAgendasAsync(clientGuid);
        if (!result.IsSuccess)
            return OperationResult<IEnumerable<AgendaDto>>.Failed(null, result.Error ?? "Agendas not retrieved", 400);

        var dtos = result.Value?.Select(a => a.ToAgendaDto()) ?? Enumerable.Empty<AgendaDto>();
        return OperationResult<IEnumerable<AgendaDto>>.Succeeded(dtos, "Agendas retrieved", 200);
    }

    public async Task<OperationResult<AgendaDto>> GetAgendaById(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<AgendaDto>.Failed(null, "Invalid ID format", 400);

        var result = await _agendaService.GetAgendaByIdAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<AgendaDto>.Failed(null, result.Error ?? "Agenda not found", 404);

        return OperationResult<AgendaDto>.Succeeded(result.Value!.ToAgendaDto(), "Agenda retrieved", 200);
    }

    #endregion

    #region Criação

    public async Task<OperationResult<AgendaDto>> CreateAgenda(AgendaDto dto)
    {
        try
        {
            var agenda = dto.ToAgenda();
            var result = await _agendaService.CreateAgendaAsync(agenda);

            if (!result.IsSuccess)
                return OperationResult<AgendaDto>.Failed(null, result.Error ?? "Agenda not created", 406);

            return OperationResult<AgendaDto>.Succeeded(agenda.ToAgendaDto(), "Agenda created", 201);
        }
        catch (Exception ex)
        {
            return OperationResult<AgendaDto>.Failed(null, ex.Message, 400);
        }
    }

    #endregion

    #region Atualizações

    public async Task<OperationResult<AgendaDto>> UpdateAgenda(string id, AgendaDto dto)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<AgendaDto>.Failed(null, "Invalid ID format", 400);

        var existingResult = await _agendaService.GetAgendaByIdAsync(guid);
        if (!existingResult.IsSuccess)
            return OperationResult<AgendaDto>.Failed(null, existingResult.Error!, 404);

        var agenda = existingResult.Value!;
        agenda.UpdateFromDto(dto);

        var updateResult = await _agendaService.UpdateAgendaAsync(agenda);
        if (!updateResult.IsSuccess)
            return OperationResult<AgendaDto>.Failed(null, updateResult.Error ?? "Agenda not updated", 400);

        return OperationResult<AgendaDto>.Succeeded(agenda.ToAgendaDto(), "Agenda updated", 200);
    }

    public async Task<OperationResult<AgendaDto>> UpdateAgendaTitle(string id, string newTitle)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<AgendaDto>.Failed(null, "Invalid ID format", 400);

        var result = await _agendaService.UpdateAgendaTitleAsync(guid, newTitle);
        if (!result.IsSuccess)
            return OperationResult<AgendaDto>.Failed(null, result.Error ?? "Title not updated", 400);

        return OperationResult<AgendaDto>.Succeeded(result.Value!.ToAgendaDto(), "Title updated", 200);
    }

    public async Task<OperationResult<AgendaDto>> UpdateAgendaConfiguration(string id, AgendaConfigurationDto dto)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<AgendaDto>.Failed(null, "Invalid ID format", 400);

        var newConfig = dto.ToConfigurationEntity();
        var result = await _agendaService.UpdateAgendaConfigurationAsync(guid, newConfig);

        if (!result.IsSuccess)
            return OperationResult<AgendaDto>.Failed(null, result.Error ?? "Configuration not updated", 400);

        return OperationResult<AgendaDto>.Succeeded(result.Value!.ToAgendaDto(), "Configuration updated", 200);
    }

    public async Task<OperationResult<AgendaDto>> UpdateAgendaSeason(string id, AgendaSeasonDto dto)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<AgendaDto>.Failed(null, "Invalid ID format", 400);

        var newSeason = dto.ToSeasonEntity();
        var result = await _agendaService.UpdateAgendaSeasonAsync(guid, newSeason);

        if (!result.IsSuccess)
            return OperationResult<AgendaDto>.Failed(null, result.Error ?? "Season not updated", 400);

        return OperationResult<AgendaDto>.Succeeded(result.Value!.ToAgendaDto(), "Season updated", 200);
    }

    #endregion

    #region Exclusão

    public async Task<OperationResult<bool>> DeleteAgenda(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<bool>.Failed(false, "Invalid ID format", 400);

        var result = await _agendaService.DeleteAgendaAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<bool>.Failed(false, result.Error ?? "Agenda not deleted", 400);

        return OperationResult<bool>.Succeeded(true, "Agenda deleted", 200);
    }

    #endregion

    #region Geração

    public async Task<OperationResult<byte[]>> GenerateAgendaFile(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<byte[]>.Failed(null, "ID com formato inválido", 400);

        var serviceResult = await _agendaService.GenerateAgendaFileAsync(guid);

        if (!serviceResult.IsSuccess)
        {
            return OperationResult<byte[]>.Failed(null, serviceResult.Error ?? "Ocorreu um erro desconhecido.", 404);         }

        return OperationResult<byte[]>.Succeeded(serviceResult.Value, "Arquivo de Agenda gerado", 200);
    }

    #endregion

}
