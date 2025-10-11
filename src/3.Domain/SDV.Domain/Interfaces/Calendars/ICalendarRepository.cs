using SDV.Domain.Entities.Calendars;

namespace SDV.Domain.Interfaces.Calendars;

public interface ICalendarRepository
{
    Task<IEnumerable<Calendar>> GetAllAsync(Guid clientId);
    Task<Calendar?> GetByIdAsync(Guid id);
    Task AddAsync(Calendar entity);
    Task UpdateAsync(Calendar entity);
    Task DeleteAsync(Guid id);
}
