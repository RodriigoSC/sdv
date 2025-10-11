using SDV.Domain.Enums;
using SDV.Domain.Enums.Commons;

namespace SDV.Application.Dtos.Agendas;

public class AgendaConfigurationDto
{
    public DayNumberFormat DayNumberFormat { get; set; }
    public WeekAbbreviation WeekFormat { get; set; }
    public MonthAbbreviation MonthFormat { get; set; }
    public FileType FileType { get; set; }
    public string Culture { get; set; } = "pt-BR";
    public DayOfWeek StartOfWeek { get; set; } = DayOfWeek.Monday;

}