using MongoDB.Bson.Serialization.Attributes;
using SDV.Domain.Entities.Calendars.ValueObjects;
using SDV.Domain.Entities.Commons;

namespace SDV.Domain.Entities.Calendars;

public class Calendar : BaseEntity
{
    public Guid ClientId { get; private set; }
    public string Title { get; private set; }

    [BsonElement("CalendarDay")]
    private List<CalendarDay> _calendars;
    public IReadOnlyCollection<CalendarDay> Calendars => _calendars.AsReadOnly();

    public Calendar(Guid clientId, string title)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId is required.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Id = Guid.NewGuid();
        ClientId = clientId;
        Title = title;
        _calendars = new List<CalendarDay>();

        MarkAsUpdated();
    }

    #region Título

    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty.", nameof(newTitle));

        if (Title != newTitle)
        {
            Title = newTitle;
            MarkAsUpdated();
        }
    }

    #endregion

    #region Feriados

    public void AddCalendar(CalendarDay calendar)
    {
        if (_calendars.Any(h => h.Date.Date == calendar.Date.Date))
            throw new ArgumentException("Holiday already exists on this date.");

        _calendars.Add(calendar);
        MarkAsUpdated();
    }

    public void RemoveCalendar(DateTime date)
    {
        var item = _calendars.FirstOrDefault(h => h.Date.Date == date.Date);
        if (item != null)
        {
            _calendars.Remove(item);
            MarkAsUpdated();
        }
    }

    public void ReplaceCalendars(IEnumerable<CalendarDay> newCalendars)
    {
        if (newCalendars == null)
            throw new ArgumentNullException(nameof(newCalendars));

        var distinct = newCalendars
            .GroupBy(h => h.Date.Date)
            .Select(g => g.First())
            .ToList();

        _calendars.Clear();
        _calendars.AddRange(distinct);
        MarkAsUpdated();
    }

    public void ClearCalendars()
    {
        if (_calendars.Any())
        {
            _calendars.Clear();
            MarkAsUpdated();
        }
    }

    #endregion
}