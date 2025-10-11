using SDV.Application.Dtos.Planners;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

public interface IPlannerApplication
{
    #region Consultas
    Task<OperationResult<IEnumerable<PlannerDto>>> GetAllPlanners(string clientId);
    Task<OperationResult<PlannerDto>> GetPlannerById(string id);
    #endregion

    #region Criação
    Task<OperationResult<PlannerDto>> CreatePlanner(PlannerDto dto);
    #endregion

    #region Atualizações
    Task<OperationResult<PlannerDto>> UpdatePlanner(string id, PlannerDto dto);
    Task<OperationResult<PlannerDto>> UpdatePlannerTitle(string id, string newTitle);
    Task<OperationResult<PlannerDto>> UpdatePlannerConfiguration(string id, PlannerConfigurationDto dto);
    Task<OperationResult<PlannerDto>> UpdatePlannerSeason(string id, PlannerSeasonDto dto);
    #endregion

    #region Exclusão
    Task<OperationResult<bool>> DeletePlanner(string id);
    #endregion

    #region Geração
    Task<OperationResult<byte[]>> GeneratePlannerFile(string id);
    #endregion
}
