using SDV.Domain.Entities.Agendas;
using SDV.Domain.Entities.Agendas.ValueObjects;
using SDV.Domain.Entities.Commons;

namespace SDV.Domain.Interfaces.Agendas;

public interface IAgendaService
{
    #region Consultas
    Task<Result<IEnumerable<Agenda>>> GetAllAgendasAsync(Guid clientId);
    Task<Result<Agenda>> GetAgendaByIdAsync(Guid id);
    #endregion

    #region Criação
    Task<Result<Agenda>> CreateAgendaAsync(Agenda agenda);
    #endregion

    #region Atualizações
    Task<Result<Agenda>> UpdateAgendaAsync(Agenda agenda);
    Task<Result<Agenda>> UpdateAgendaTitleAsync(Guid id, string newTitle);
    Task<Result<Agenda>> UpdateAgendaConfigurationAsync(Guid id, AgendaConfiguration newConfiguration);
    Task<Result<Agenda>> UpdateAgendaSeasonAsync(Guid id, AgendaSeason newSeason);
    #endregion

    #region Exclusão
    Task<Result<bool>> DeleteAgendaAsync(Guid id);
    #endregion

    #region Geração de Arquivo
    Task<Result<byte[]>> GenerateAgendaFileAsync(Guid agendaId);
    
    #endregion

}
