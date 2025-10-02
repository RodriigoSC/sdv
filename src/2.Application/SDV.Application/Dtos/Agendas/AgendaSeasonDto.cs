using SDV.Domain.Enums.Commons;

namespace SDV.Application.Dtos.Agendas;

public class AgendaSeasonDto
{
    public List<Weekday> DaysOfWeek { get; set; } = new();
    public Dictionary<int, List<Month>> YearsMonths { get; set; } = new();
}