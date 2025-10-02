using SDV.Application.Dtos.Agendas;
using SDV.Domain.Entities.Agendas;
using SDV.Domain.Entities.Agendas.ValueObjects;

namespace SDV.Application.Mappers;

public static class AgendaMapper
{
    // Entity -> DTO
    public static AgendaDto ToAgendaDto(this Agenda agenda)
    {
        if (agenda == null) return null!;

        return new AgendaDto
        {
            Id = agenda.Id.ToString(),
            ClientId = agenda.ClientId.ToString(),
            Title = agenda.Title,
            AgendaType = agenda.AgendaType,
            Configuration = agenda.Configuration.ToConfigurationDto(),
            Season = agenda.Season.ToSeasonDto(),
            MessageTemplateId = agenda.MessageTemplateId?.ToString(),
            CalendarTemplateId = agenda.CalendarTemplateId?.ToString()
        };
    }

    public static IEnumerable<AgendaDto> ToAgendaDtoList(this IEnumerable<Agenda> agendas)
    {
        if (agendas == null) yield break;
        foreach (var agenda in agendas)
            yield return agenda.ToAgendaDto();
    }

    // DTO -> Entity (for creation)
    public static Agenda ToAgenda(this AgendaDto dto)
    {
        if (dto == null) return null!;

        var agenda = new Agenda(
            clientId: Guid.Parse(dto.ClientId),
            title: dto.Title,
            agendaType: dto.AgendaType
        );

        agenda.UpdateConfiguration(dto.Configuration.ToConfigurationEntity());
        agenda.UpdateSeason(dto.Season.ToSeasonEntity());

        if (!string.IsNullOrWhiteSpace(dto.MessageTemplateId))
            agenda.SetTemplates(
                Guid.Parse(dto.MessageTemplateId),
                string.IsNullOrWhiteSpace(dto.CalendarTemplateId) ? null : Guid.Parse(dto.CalendarTemplateId)
            );

        return agenda;
    }

    public static IEnumerable<Agenda> ToAgendaList(this IEnumerable<AgendaDto> dtos)
    {
        if (dtos == null) yield break;
        foreach (var dto in dtos)
            yield return dto.ToAgenda();
    }

    // Update existing entity from DTO
    public static void UpdateFromDto(this Agenda agenda, AgendaDto dto)
    {
        if (agenda == null || dto == null) return;

        if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != agenda.Title)
            agenda.ChangeTitle(dto.Title);

        agenda.ChangeAgendaType(dto.AgendaType);

        if (dto.Configuration != null)
            agenda.UpdateConfiguration(dto.Configuration.ToConfigurationEntity());

        if (dto.Season != null)
            agenda.UpdateSeason(dto.Season.ToSeasonEntity());

        if (!string.IsNullOrWhiteSpace(dto.MessageTemplateId) || !string.IsNullOrWhiteSpace(dto.CalendarTemplateId))
            agenda.SetTemplates(
                string.IsNullOrWhiteSpace(dto.MessageTemplateId) ? null : Guid.Parse(dto.MessageTemplateId),
                string.IsNullOrWhiteSpace(dto.CalendarTemplateId) ? null : Guid.Parse(dto.CalendarTemplateId)
            );
    }

    #region Sub-mappers

    private static AgendaConfigurationDto ToConfigurationDto(this AgendaConfiguration config)
        => new AgendaConfigurationDto
        {
            DayNumberFormat = config.DayNumberFormat,
            WeekFormat = config.WeekAbbreviation,
            MonthFormat = config.MonthAbbreviation,
            FileType = config.FileType,
            Culture = config.Culture,
            StartOfWeek = config.StartOfWeek
        };

    public static AgendaConfiguration ToConfigurationEntity(this AgendaConfigurationDto dto)
        => new AgendaConfiguration(dto.DayNumberFormat, dto.WeekFormat, dto.MonthFormat, dto.FileType, dto.Culture, dto.StartOfWeek);

    public static AgendaSeasonDto ToSeasonDto(this AgendaSeason season)
        => new AgendaSeasonDto
        {
            DaysOfWeek = season.DaysOfWeek.ToList(),
            // Convert string keys to int in DTO
            YearsMonths = season.YearsMonths.ToDictionary(k => int.Parse(k.Key), v => v.Value.ToList())
        };

    public static AgendaSeason ToSeasonEntity(this AgendaSeasonDto dto)
    {
        // Convert int keys from DTO to string in entity
        var yearsMonths = dto.YearsMonths.ToDictionary(k => k.Key.ToString(), v => v.Value);
        return new AgendaSeason(dto.DaysOfWeek, yearsMonths);
    }

    #endregion
}