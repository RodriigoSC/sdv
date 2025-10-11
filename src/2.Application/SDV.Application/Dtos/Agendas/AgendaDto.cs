using SDV.Application.Dtos.Commons;
using SDV.Domain.Enums.Agendas;

namespace SDV.Application.Dtos.Agendas;

public class AgendaDto : ICommonDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public AgendaType AgendaType { get; set; }
    public AgendaConfigurationDto Configuration { get; set; } = new();
    public AgendaSeasonDto Season { get; set; } = new();
    public string? MessageTemplateId { get; set; }
    public string? CalendarTemplateId { get; set; }
}
