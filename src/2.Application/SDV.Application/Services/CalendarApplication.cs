using SDV.Application.Dtos.Calendars;
using SDV.Application.Interfaces;
using SDV.Application.Mappers;
using SDV.Application.Results;
using SDV.Domain.Interfaces.Calendars;

namespace SDV.Application.Services;

public class CalendarApplication : ICalendarApplication
{
    private readonly ICalendarService _calendarService;

    public CalendarApplication(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    #region Consultas

    public async Task<OperationResult<IEnumerable<CalendarDto>>> GetAllCalendars(string clientId)
    {
        if (!Guid.TryParse(clientId, out var clientGuid))
            return OperationResult<IEnumerable<CalendarDto>>.Failed(null, "Invalid client ID format", 400);

        var result = await _calendarService.GetAllCalendarsAsync(clientGuid);
        if (!result.IsSuccess)
            return OperationResult<IEnumerable<CalendarDto>>.Failed(null, result.Error ?? "Calendars not retrieved", 400);

        var dtos = result.Value?.ToCalendarDtoList() ?? Enumerable.Empty<CalendarDto>();
        return OperationResult<IEnumerable<CalendarDto>>.Succeeded(dtos, "Calendars retrieved", 200);
    }

    public async Task<OperationResult<CalendarDto>> GetCalendarById(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<CalendarDto>.Failed(null, "Invalid ID format", 400);

        var result = await _calendarService.GetCalendarByIdAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<CalendarDto>.Failed(null, result.Error ?? "Calendar not found", 404);

        return OperationResult<CalendarDto>.Succeeded(result.Value!.ToCalendarDto(), "Calendar retrieved", 200);
    }

    #endregion

    #region Criação

    public async Task<OperationResult<CalendarDto>> CreateCalendar(CalendarDto dto)
    {
        try
        {
            var calendar = dto.ToCalendar();
            var result = await _calendarService.CreateCalendarAsync(calendar);

            if (!result.IsSuccess)
                return OperationResult<CalendarDto>.Failed(null, result.Error ?? "Calendar not created", 406);

            return OperationResult<CalendarDto>.Succeeded(calendar.ToCalendarDto(), "Calendar created", 201);
        }
        catch (Exception ex)
        {
            return OperationResult<CalendarDto>.Failed(null, ex.Message, 400);
        }
    }

    #endregion

    #region Atualizações

    public async Task<OperationResult<CalendarDto>> UpdateCalendar(string id, CalendarDto dto)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<CalendarDto>.Failed(null, "Invalid ID format", 400);

        var existingResult = await _calendarService.GetCalendarByIdAsync(guid);
        if (!existingResult.IsSuccess)
            return OperationResult<CalendarDto>.Failed(null, existingResult.Error!, 404);

        var calendar = existingResult.Value!;
        calendar.UpdateFromDto(dto);

        var updateResult = await _calendarService.UpdateCalendarAsync(calendar);
        if (!updateResult.IsSuccess)
            return OperationResult<CalendarDto>.Failed(null, updateResult.Error ?? "Calendar not updated", 400);

        return OperationResult<CalendarDto>.Succeeded(calendar.ToCalendarDto(), "Calendar updated", 200);
    }


    #endregion

    #region Exclusão

    public async Task<OperationResult<bool>> DeleteCalendar(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<bool>.Failed(false, "Invalid ID format", 400);

        var result = await _calendarService.DeleteCalendarAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<bool>.Failed(false, result.Error ?? "Calendar not deleted", 400);

        return OperationResult<bool>.Succeeded(true, "Calendar deleted", 200);
    }

    #endregion
}
