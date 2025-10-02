namespace SDV.Application.Dtos.Calendars;

public class CalendarDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<CalendarDayDto> CalendarDays { get; set; } = new();

}
