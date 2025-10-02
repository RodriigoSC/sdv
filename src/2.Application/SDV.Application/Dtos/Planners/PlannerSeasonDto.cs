using SDV.Domain.Enums.Commons;

namespace SDV.Application.Dtos.Planners;

public class PlannerSeasonDto
{
    public List<Weekday> DaysOfWeek { get; set; } = new();
    public Dictionary<int, List<Month>> YearsMonths { get; set; } = new();
}