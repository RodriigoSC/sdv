using SDV.Application.Dtos.Agendas;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

public interface IAgendaApplication
{
    #region Consultas
    Task<OperationResult<IEnumerable<AgendaDto>>> GetAllAgendas(string clientId);
    Task<OperationResult<AgendaDto>> GetAgendaById(string id);
    #endregion

    #region Criação
    Task<OperationResult<AgendaDto>> CreateAgenda(AgendaDto dto);
    #endregion

    #region Atualizações
    Task<OperationResult<AgendaDto>> UpdateAgenda(string id, AgendaDto dto);
    Task<OperationResult<AgendaDto>> UpdateAgendaTitle(string id, string newTitle);
    Task<OperationResult<AgendaDto>> UpdateAgendaConfiguration(string id, AgendaConfigurationDto dto);
    Task<OperationResult<AgendaDto>> UpdateAgendaSeason(string id, AgendaSeasonDto dto);
    #endregion

    #region Exclusão
    Task<OperationResult<bool>> DeleteAgenda(string id);
    #endregion

    #region Geração
    Task<OperationResult<byte[]>> GenerateAgendaFile(string id);
    #endregion

}
