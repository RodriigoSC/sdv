using SDV.Domain.Entities.Agendas.ValueObjects;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Enums.Agendas;

namespace SDV.Domain.Entities.Agendas;

public class Agenda : BaseEntity
{    
    public Guid ClientId { get; private set; }
    public string Title { get; private set; }
    public AgendaType AgendaType { get; private set; }
    public AgendaConfiguration Configuration { get; private set; }
    public AgendaSeason Season { get; private set; }    
    public Guid? MessageTemplateId { get; private set; }    
    public Guid? CalendarTemplateId { get; private set; }

    public Agenda(Guid clientId, string title, AgendaType agendaType)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId is required.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Id = Guid.NewGuid();
        ClientId = clientId;
        Title = title;
        AgendaType = agendaType;
        Configuration = AgendaConfiguration.Default();
        Season = new AgendaSeason();

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

    public void ChangeAgendaType(AgendaType newAgendaType)
    {
        if (newAgendaType == AgendaType) return;

        AgendaType = newAgendaType;
        MarkAsUpdated();
    }

    public void SetTemplates(Guid? messageTemplateId, Guid? calendarTemplateId)
    {
        MessageTemplateId = messageTemplateId;
        CalendarTemplateId = calendarTemplateId;
        MarkAsUpdated();
    }

    public void UpdateConfiguration(AgendaConfiguration newConfig)
    {
        Configuration = newConfig ?? throw new ArgumentNullException(nameof(newConfig));
        MarkAsUpdated();
    }

    public void UpdateSeason(AgendaSeason newSeason)
    {
        Season = newSeason ?? throw new ArgumentNullException(nameof(newSeason));
        MarkAsUpdated();
    }
}