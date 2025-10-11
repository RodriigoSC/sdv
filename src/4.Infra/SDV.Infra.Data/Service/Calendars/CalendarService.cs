using System;
using SDV.Domain.Entities.Calendars;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Exceptions;
using SDV.Domain.Interfaces.Calendars;
using SDV.Domain.Specification;

namespace SDV.Infra.Data.Service.Calendars;

public class CalendarService : ICalendarService
{
    private readonly ICalendarRepository _calendarRepository;

    public CalendarService(ICalendarRepository calendarRepository)
    {
        _calendarRepository = calendarRepository;
    }

    #region Consultas

    public async Task<Result<IEnumerable<Calendar>>> GetAllCalendarsAsync(Guid userId)
    {
        var calendars = await _calendarRepository.GetAllAsync(userId);
        return Result<IEnumerable<Calendar>>.Success(calendars);
    }

    public async Task<Result<Calendar>> GetCalendarByIdAsync(Guid id)
    {
        var calendar = await _calendarRepository.GetByIdAsync(id);
        return calendar is null
            ? Result<Calendar>.Failure("Feriado não encontrado")
            : Result<Calendar>.Success(calendar);
    }

    #endregion

    #region Criação

    public async Task<Result<Calendar>> CreateCalendarAsync(Calendar calendar)
    {
        try
        {
            new CalendarValidationSpecification().IsValid(calendar);
        }
        catch (EntityValidationException ex)
        {
            return Result<Calendar>.Failure(ex.Message);
        }

        await _calendarRepository.AddAsync(calendar);
        return Result<Calendar>.Success(calendar);
    }

    #endregion

    #region Atualizações

    public async Task<Result<Calendar>> UpdateCalendarAsync(Calendar calendar)
    {
        var existing = await _calendarRepository.GetByIdAsync(calendar.Id);
        if (existing is null)
            return Result<Calendar>.Failure("Feriado não encontrado");

        try
        {
            new CalendarValidationSpecification().IsValid(calendar);
        }
        catch (EntityValidationException ex)
        {
            return Result<Calendar>.Failure(ex.Message);
        }

        await _calendarRepository.UpdateAsync(calendar);
        return Result<Calendar>.Success(calendar);
    }


    #endregion

    #region Exclusão

    public async Task<Result<bool>> DeleteCalendarAsync(Guid id)
    {
        var calendar = await _calendarRepository.GetByIdAsync(id);
        if (calendar is null)
            return Result<bool>.Failure("Feriado não encontrado");

        await _calendarRepository.DeleteAsync(id);
        return Result<bool>.Success(true);
    }

    #endregion
}
