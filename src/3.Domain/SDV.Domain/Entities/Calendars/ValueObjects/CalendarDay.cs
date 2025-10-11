namespace SDV.Domain.Entities.Calendars.ValueObjects;

public record CalendarDay
{
    public string Content { get; init; }
    public DateTime Date { get; init; }

    public CalendarDay(string content, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));

        Content = content;
        Date = date.Date;
    }
}