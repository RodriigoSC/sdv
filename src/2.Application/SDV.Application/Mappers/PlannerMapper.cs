using SDV.Application.Dtos.Planners;
using SDV.Domain.Entities.Planners;
using SDV.Domain.Entities.Planners.ValueObjects;

namespace SDV.Application.Mappers;

public static class PlannerMapper
{
    // Entity -> DTO
    public static PlannerDto ToPlannerDto(this Planner planner)
    {
        if (planner == null) return null!;

        return new PlannerDto
        {
            Id = planner.Id.ToString(),
            ClientId = planner.ClientId.ToString(),
            Title = planner.Title,
            PlannerType = planner.PlannerType,
            Configuration = planner.Configuration.ToConfigurationDto(),
            Season = planner.Season.ToSeasonDto(),
            MessageTemplateId = planner.MessageTemplateId?.ToString(),
            CalendarTemplateId = planner.CalendarTemplateId?.ToString()
        };
    }

    public static IEnumerable<PlannerDto> ToPlannerDtoList(this IEnumerable<Planner> planners)
    {
        if (planners == null) yield break;
        foreach (var planner in planners)
            yield return planner.ToPlannerDto();
    }

    // DTO -> Entity (for creation)
    public static Planner ToPlanner(this PlannerDto dto)
    {
        if (dto == null) return null!;

        var planner = new Planner(
            clientId: Guid.Parse(dto.ClientId),
            title: dto.Title,
            plannerType: dto.PlannerType
        );

        planner.UpdateConfiguration(dto.Configuration.ToConfigurationEntity());
        planner.UpdateSeason(dto.Season.ToSeasonEntity());

        if (!string.IsNullOrWhiteSpace(dto.MessageTemplateId))
            planner.SetTemplates(
                Guid.Parse(dto.MessageTemplateId),
                string.IsNullOrWhiteSpace(dto.CalendarTemplateId) ? null : Guid.Parse(dto.CalendarTemplateId)
            );

        return planner;
    }

    public static IEnumerable<Planner> ToPlannerList(this IEnumerable<PlannerDto> dtos)
    {
        if (dtos == null) yield break;
        foreach (var dto in dtos)
            yield return dto.ToPlanner();
    }

    // Update existing entity from DTO
    public static void UpdateFromDto(this Planner planner, PlannerDto dto)
    {
        if (planner == null || dto == null) return;

        if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != planner.Title)
            planner.ChangeTitle(dto.Title);

        planner.ChangePlannerType(dto.PlannerType);

        if (dto.Configuration != null)
            planner.UpdateConfiguration(dto.Configuration.ToConfigurationEntity());

        if (dto.Season != null)
            planner.UpdateSeason(dto.Season.ToSeasonEntity());

        if (!string.IsNullOrWhiteSpace(dto.MessageTemplateId) || !string.IsNullOrWhiteSpace(dto.CalendarTemplateId))
            planner.SetTemplates(
                string.IsNullOrWhiteSpace(dto.MessageTemplateId) ? null : Guid.Parse(dto.MessageTemplateId),
                string.IsNullOrWhiteSpace(dto.CalendarTemplateId) ? null : Guid.Parse(dto.CalendarTemplateId)
            );
    }

    #region Sub-mappers

    private static PlannerConfigurationDto ToConfigurationDto(this PlannerConfiguration config)
        => new PlannerConfigurationDto
        {
            DayNumberFormat = config.DayNumberFormat,
            WeekFormat = config.WeekAbbreviation,
            MonthFormat = config.MonthAbbreviation,
            FileType = config.FileType,
            Culture = config.Culture,
            StartOfWeek = config.StartOfWeek
        };

    public static PlannerConfiguration ToConfigurationEntity(this PlannerConfigurationDto dto)
        => new PlannerConfiguration(dto.DayNumberFormat, dto.WeekFormat, dto.MonthFormat, dto.FileType, dto.Culture, dto.StartOfWeek);

    public static PlannerSeasonDto ToSeasonDto(this PlannerSeason season)
        => new PlannerSeasonDto
        {
            DaysOfWeek = season.DaysOfWeek.ToList(),
            // Convert string keys to int in DTO
            YearsMonths = season.YearsMonths.ToDictionary(k => int.Parse(k.Key), v => v.Value.ToList())
        };

    public static PlannerSeason ToSeasonEntity(this PlannerSeasonDto dto)
    {
        // Convert int keys from DTO to string in entity
        var yearsMonths = dto.YearsMonths.ToDictionary(k => k.Key.ToString(), v => v.Value);
        return new PlannerSeason(dto.DaysOfWeek, yearsMonths);
    }

    #endregion
}