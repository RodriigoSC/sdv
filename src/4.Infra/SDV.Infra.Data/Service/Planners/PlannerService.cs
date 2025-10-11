using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Planners;
using SDV.Domain.Entities.Planners.ValueObjects;
using SDV.Domain.Exceptions;
using SDV.Domain.Interfaces.Planners;
using SDV.Domain.Specification;
using SDV.Infra.File;

namespace SDV.Infra.Data.Service.Planners;

public class PlannerService : IPlannerService
{

    private readonly IPlannerRepository _plannerRepository;
    private readonly IPlannerFileGeneratorService _fileGenerator;

    public PlannerService(IPlannerRepository plannerRepository, IPlannerFileGeneratorService fileGenerator)
    {
        _plannerRepository = plannerRepository;
        _fileGenerator = fileGenerator;
    }

    #region Consultas

    public async Task<Result<IEnumerable<Planner>>> GetAllPlannersAsync(Guid clientId)
    {
        var planners = await _plannerRepository.GetAllAsync(clientId);
        return Result<IEnumerable<Planner>>.Success(planners);
    }

    public async Task<Result<Planner>> GetPlannerByIdAsync(Guid id)
    {
        var planner = await _plannerRepository.GetByIdAsync(id);
        return planner is null
            ? Result<Planner>.Failure("Planner não encontrado")
            : Result<Planner>.Success(planner);
    }

    #endregion

    #region Criação

    public async Task<Result<Planner>> CreatePlannerAsync(Planner planner)
    {
        try
        {
            new PlannerValidationSpecification().IsValid(planner);
        }
        catch (EntityValidationException ex)
        {
            return Result<Planner>.Failure(ex.Message);
        }

        await _plannerRepository.AddAsync(planner);
        return Result<Planner>.Success(planner);
    }

    #endregion

    #region Atualizações

    public async Task<Result<Planner>> UpdatePlannerAsync(Planner planner)
    {
        try
        {
            new PlannerValidationSpecification().IsValid(planner);
        }
        catch (EntityValidationException ex)
        {
            return Result<Planner>.Failure(ex.Message);
        }

        await _plannerRepository.UpdateAsync(planner);
        return Result<Planner>.Success(planner);
    }

    public async Task<Result<Planner>> UpdatePlannerTitleAsync(Guid id, string newTitle)
    {
        var planner = await _plannerRepository.GetByIdAsync(id);
        if (planner is null)
            return Result<Planner>.Failure("Planner não encontrada");

        planner.ChangeTitle(newTitle);
        await _plannerRepository.UpdateAsync(planner);

        return Result<Planner>.Success(planner);
    }

    public async Task<Result<Planner>> UpdatePlannerConfigurationAsync(Guid id, PlannerConfiguration newConfiguration)
    {
        var planner = await _plannerRepository.GetByIdAsync(id);
        if (planner is null)
            return Result<Planner>.Failure("Planner não encontrado");

        planner.UpdateConfiguration(newConfiguration);
        await _plannerRepository.UpdateAsync(planner);

        return Result<Planner>.Success(planner);
    }

    public async Task<Result<Planner>> UpdatePlannerSeasonAsync(Guid id, PlannerSeason newSeason)
    {
        var planner = await _plannerRepository.GetByIdAsync(id);
        if (planner is null)
            return Result<Planner>.Failure("Planner não encontrado");

        planner.UpdateSeason(newSeason);
        await _plannerRepository.UpdateAsync(planner);

        return Result<Planner>.Success(planner);
    }

    #endregion

    #region Exclusão

    public async Task<Result<bool>> DeletePlannerAsync(Guid id)
    {
        var planner = await _plannerRepository.GetByIdAsync(id);
        if (planner is null)
            return Result<bool>.Failure("Planner não encontrado");

        await _plannerRepository.DeleteAsync(id);
        return Result<bool>.Success(true);
    }

    #endregion

    #region Geração

    public async Task<Result<byte[]>> GeneratePlannerFileAsync(Guid plannerId)
    {
        var planner = await _plannerRepository.GetByIdAsync(plannerId);
        if (planner == null) 
            return Result<byte[]>.Failure("Planner não encontrado");

        var data = await _fileGenerator.GeneratePlannerDataAsync(planner);
        var csvBytes = _fileGenerator.GeneratePlannerCsv(planner, data);

        return Result<byte[]>.Success(csvBytes);
    }

    #endregion
    
    

}
