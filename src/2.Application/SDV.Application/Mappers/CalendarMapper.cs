using SDV.Domain.Entities.Calendars;
using SDV.Application.Dtos.Calendars;
using SDV.Domain.Entities.Calendars.ValueObjects;

namespace SDV.Application.Mappers;

public static class CalendarMapper
{
    // Entidade -> DTO
    public static CalendarDto ToCalendarDto(this Calendar calendar)
    {
        if (calendar == null) return null!;

        return new CalendarDto
        {
            Id = calendar.Id.ToString(),
            ClientId = calendar.ClientId.ToString(),
            Title = calendar.Title,
            CalendarDays = calendar.Calendars?
                .Select(h => new CalendarDayDto
                {
                    Content = h.Content,
                    Date = h.Date
                })
                .ToList() ?? new List<CalendarDayDto>()
        };
    }

    // DTO -> Entidade
    public static Calendar ToCalendar(this CalendarDto dto)
    {
        if (dto == null) return null!;

        var calendar = new Calendar(Guid.Parse(dto.ClientId), dto.Title);

        if (dto.CalendarDays != null && dto.CalendarDays.Any())
        {
            var calendars = dto.CalendarDays
                .Select(h => new CalendarDay(h.Content, h.Date))
                .ToList();

            calendar.ReplaceCalendars(calendars);
        }

        return calendar;
    }

    // Atualiza entidade existente a partir do DTO
    public static void UpdateFromDto(this Calendar calendar, CalendarDto dto)
    {
        if (calendar == null || dto == null) return;

        if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != calendar.Title)
            calendar.UpdateTitle(dto.Title);

        var calendars = dto.CalendarDays != null
            ? dto.CalendarDays.Select(h => new CalendarDay(h.Content, h.Date)).ToList()
            : new List<CalendarDay>();

        calendar.ReplaceCalendars(calendars);
    }


    public static IEnumerable<CalendarDto> ToCalendarDtoList(this IEnumerable<Calendar> calendars)
        => calendars.Select(c => c.ToCalendarDto());
}
