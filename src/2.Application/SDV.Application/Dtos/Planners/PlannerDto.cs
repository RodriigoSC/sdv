using SDV.Application.Dtos.Commons;
using SDV.Domain.Enums.Planners;

namespace SDV.Application.Dtos.Planners;

public class PlannerDto : ICommonDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public PlannerType PlannerType { get; set; }
    public PlannerConfigurationDto Configuration { get; set; } = new();
    public PlannerSeasonDto Season { get; set; } = new();
    public string? MessageTemplateId { get; set; }
    public string? CalendarTemplateId { get; set; }
}
