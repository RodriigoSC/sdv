using SDV.Application.Dtos.Calendars;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

public interface ICalendarApplication
{
    #region Consultas
    Task<OperationResult<IEnumerable<CalendarDto>>> GetAllCalendars(string clientId);
    Task<OperationResult<CalendarDto>> GetCalendarById(string id);
    #endregion

    #region Criação
    Task<OperationResult<CalendarDto>> CreateCalendar(CalendarDto dto);
    #endregion

    #region Atualizações
    Task<OperationResult<CalendarDto>> UpdateCalendar(string id, CalendarDto dto);
    
    #endregion

    #region Exclusão
    Task<OperationResult<bool>> DeleteCalendar(string id);
    #endregion

}
