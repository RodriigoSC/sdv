using SDV.Domain.Entities.Agendas;
using SDV.Domain.Entities.Agendas.ValueObjects;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Exceptions;
using SDV.Domain.Interfaces.Agendas;
using SDV.Domain.Specification;
using SDV.Infra.File;

namespace SDV.Infra.Data.Service.Agendas;

public class AgendaService : IAgendaService
{
    private readonly IAgendaRepository _agendaRepository;
    private readonly IAgendaFileGeneratorService _fileGenerator;


    public AgendaService(IAgendaRepository agendaRepository, IAgendaFileGeneratorService fileGenerator)
    {
        _agendaRepository = agendaRepository;
        _fileGenerator = fileGenerator;
    }

    #region Consultas

    public async Task<Result<IEnumerable<Agenda>>> GetAllAgendasAsync(Guid clientId)
    {
        var agendas = await _agendaRepository.GetAllAsync(clientId);
        return Result<IEnumerable<Agenda>>.Success(agendas);
    }

    public async Task<Result<Agenda>> GetAgendaByIdAsync(Guid id)
    {
        var agenda = await _agendaRepository.GetByIdAsync(id);
        return agenda is null
            ? Result<Agenda>.Failure("Agenda não encontrada")
            : Result<Agenda>.Success(agenda);
    }

    #endregion

    #region Criação

    public async Task<Result<Agenda>> CreateAgendaAsync(Agenda agenda)
    {
        try
        {
            new AgendaValidationSpecification().IsValid(agenda);
        }
        catch (EntityValidationException ex)
        {
            return Result<Agenda>.Failure(ex.Message);
        }

        await _agendaRepository.AddAsync(agenda);
        return Result<Agenda>.Success(agenda);
    }

    #endregion

    #region Atualizações

    public async Task<Result<Agenda>> UpdateAgendaAsync(Agenda agenda)
    {
        try
        {
            new AgendaValidationSpecification().IsValid(agenda);
        }
        catch (EntityValidationException ex)
        {
            return Result<Agenda>.Failure(ex.Message);
        }

        await _agendaRepository.UpdateAsync(agenda);
        return Result<Agenda>.Success(agenda);
    }

    public async Task<Result<Agenda>> UpdateAgendaTitleAsync(Guid id, string newTitle)
    {
        var agenda = await _agendaRepository.GetByIdAsync(id);
        if (agenda is null)
            return Result<Agenda>.Failure("Agenda não encontrada");

        agenda.ChangeTitle(newTitle);
        await _agendaRepository.UpdateAsync(agenda);

        return Result<Agenda>.Success(agenda);
    }

    public async Task<Result<Agenda>> UpdateAgendaConfigurationAsync(Guid id, AgendaConfiguration newConfiguration)
    {
        var agenda = await _agendaRepository.GetByIdAsync(id);
        if (agenda is null)
            return Result<Agenda>.Failure("Agenda não encontrada");

        agenda.UpdateConfiguration(newConfiguration);
        await _agendaRepository.UpdateAsync(agenda);

        return Result<Agenda>.Success(agenda);
    }

    public async Task<Result<Agenda>> UpdateAgendaSeasonAsync(Guid id, AgendaSeason newSeason)
    {
        var agenda = await _agendaRepository.GetByIdAsync(id);
        if (agenda is null)
            return Result<Agenda>.Failure("Agenda não encontrada");

        agenda.UpdateSeason(newSeason);
        await _agendaRepository.UpdateAsync(agenda);

        return Result<Agenda>.Success(agenda);
    }

    #endregion

    #region Exclusão

    public async Task<Result<bool>> DeleteAgendaAsync(Guid id)
    {
        var agenda = await _agendaRepository.GetByIdAsync(id);
        if (agenda is null)
            return Result<bool>.Failure("Agenda não encontrada");

        await _agendaRepository.DeleteAsync(id);
        return Result<bool>.Success(true);
    }

    #endregion

    #region Geração

    public async Task<Result<byte[]>> GenerateAgendaFileAsync(Guid agendaId)
    {
        var agenda = await _agendaRepository.GetByIdAsync(agendaId);
        if (agenda == null)
            return Result<byte[]>.Failure("Agenda não encontrada");

        var data = await _fileGenerator.GenerateAgendaDataAsync(agenda);
        var csvBytes = _fileGenerator.GenerateAgendaCsv(agenda, data);
        
        return Result<byte[]>.Success(csvBytes);
    }
    
    #endregion
}
