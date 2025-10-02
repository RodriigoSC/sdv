using System;

namespace SDV.Infra.File.Model;

public class PlannerFile
{
    public DateTime Date { get; set; }
    public string? DayOfWeek { get; set; } = string.Empty;
    public string? FormattedDayAndMonth { get; set; } = string.Empty;
    public string? CalendarName { get; set; }
    public string? MessageName { get; set; }

}
