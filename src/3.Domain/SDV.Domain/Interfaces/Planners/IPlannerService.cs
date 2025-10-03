using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Planners;
using SDV.Domain.Entities.Planners.ValueObjects;

namespace SDV.Domain.Interfaces.Planners;

public interface IPlannerService
{
    #region Consultas
    Task<Result<IEnumerable<Planner>>> GetAllPlannersAsync(Guid clientId);
    Task<Result<Planner>> GetPlannerByIdAsync(Guid id);
    #endregion

    #region Criação
    Task<Result<Planner>> CreatePlannerAsync(Planner planner);
    #endregion

    #region Atualizações
    Task<Result<Planner>> UpdatePlannerAsync(Planner planner);
    Task<Result<Planner>> UpdatePlannerTitleAsync(Guid id, string newTitle);
    Task<Result<Planner>> UpdatePlannerConfigurationAsync(Guid id, PlannerConfiguration newConfiguration);
    Task<Result<Planner>> UpdatePlannerSeasonAsync(Guid id, PlannerSeason newSeason);
    #endregion

    #region Exclusão
    Task<Result<bool>> DeletePlannerAsync(Guid id);
    #endregion

    #region Geração de Arquivo
    Task<Result<byte[]>> GeneratePlannerFileAsync(Guid plannerId);
    
    #endregion

}
