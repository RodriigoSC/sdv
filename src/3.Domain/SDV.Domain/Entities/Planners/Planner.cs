using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Planners.ValueObjects;
using SDV.Domain.Enums.Planners;

namespace SDV.Domain.Entities.Planners;

public class Planner : BaseEntity
{
    public Guid ClientId { get; private set; }
    public string Title { get; private set; }
    public PlannerType PlannerType { get; private set; }
    public PlannerConfiguration Configuration { get; private set; }
    public PlannerSeason Season { get; private set; }    
    public Guid? MessageTemplateId { get; private set; }    
    public Guid? CalendarTemplateId { get; private set; }

    public Planner(Guid clientId, string title, PlannerType plannerType)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId is required.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Id = Guid.NewGuid();
        ClientId = clientId;
        Title = title;
        PlannerType = plannerType;
        Configuration = PlannerConfiguration.Default();
        Season = new PlannerSeason();

        MarkAsUpdated();
    }

    public void ChangeTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty.", nameof(newTitle));
        if (newTitle == Title) return;

        Title = newTitle;
        MarkAsUpdated();
    }

    public void ChangePlannerType(PlannerType newPlannerType)
    {
        if (newPlannerType == PlannerType) return;

        PlannerType = newPlannerType;
        MarkAsUpdated();
    }

    public void SetTemplates(Guid? messageTemplateId, Guid? calendarTemplateId)
    {
        MessageTemplateId = messageTemplateId;
        CalendarTemplateId = calendarTemplateId;
        MarkAsUpdated();
    }

    public void UpdateConfiguration(PlannerConfiguration newConfig)
    {
        Configuration = newConfig ?? throw new ArgumentNullException(nameof(newConfig));
        MarkAsUpdated();
    }

    public void UpdateSeason(PlannerSeason newSeason)
    {
        Season = newSeason ?? throw new ArgumentNullException(nameof(newSeason));
        MarkAsUpdated();
    }
}
