using SDV.Domain.Entities.Calendars;
using SDV.Domain.Entities.Commons;

namespace SDV.Domain.Interfaces.Calendars;

public interface ICalendarService
{
    #region Consultas
    Task<Result<IEnumerable<Calendar>>> GetAllCalendarsAsync(Guid userId);
    Task<Result<Calendar>> GetCalendarByIdAsync(Guid id);
    #endregion

    #region Criação
    Task<Result<Calendar>> CreateCalendarAsync(Calendar calendar);
    #endregion

    #region Atualizações
    Task<Result<Calendar>> UpdateCalendarAsync(Calendar calendar);
    #endregion

    #region Exclusão
    Task<Result<bool>> DeleteCalendarAsync(Guid id);    
    #endregion
}
