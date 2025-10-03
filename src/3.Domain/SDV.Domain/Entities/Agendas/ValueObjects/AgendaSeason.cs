using MongoDB.Bson.Serialization.Attributes;
using SDV.Domain.Enums.Commons;

namespace SDV.Domain.Entities.Agendas.ValueObjects;

public sealed class AgendaSeason : IEquatable<AgendaSeason>
{
    [BsonElement("DaysOfWeek")]
    public List<Weekday> _daysOfWeek;
    public IReadOnlyList<Weekday> DaysOfWeek => _daysOfWeek.AsReadOnly();

    [BsonElement("YearsMonths")]
    public Dictionary<string, List<Month>> _yearsMonths;
    public IReadOnlyDictionary<string, List<Month>> YearsMonths => _yearsMonths;
    private readonly int _hashCode;


    public AgendaSeason()
    {
        _daysOfWeek = new List<Weekday>();
        _yearsMonths = new Dictionary<string, List<Month>>();
    }

    public AgendaSeason(IEnumerable<Weekday>? daysOfWeek, Dictionary<string, List<Month>>? yearsMonths)
    {
        _daysOfWeek = daysOfWeek?.Distinct().ToList() ?? new List<Weekday>();
        _yearsMonths = yearsMonths ?? new Dictionary<string, List<Month>>();

        var hash = new HashCode();
        foreach (var day in _daysOfWeek.OrderBy(d => d))
            hash.Add(day);
        foreach (var pair in _yearsMonths.OrderBy(k => k.Key))
        {
            hash.Add(pair.Key);
            foreach (var month in pair.Value.OrderBy(m => m))
                hash.Add(month);
        }
        _hashCode = hash.ToHashCode();
    }

    public AgendaSeason SetDaysOfWeek(IEnumerable<Weekday> days)
    {
        return new AgendaSeason(days, _yearsMonths);
    }

    public AgendaSeason SetYearMonths(int year, IEnumerable<Month> months)
    {
        var copy = new Dictionary<string, List<Month>>(_yearsMonths);
        copy[year.ToString()] = months.Distinct().ToList();
        return new AgendaSeason(_daysOfWeek, copy);
    }

    public bool Equals(AgendaSeason? other)
    {
        if (other is null) return false;
        return DaysOfWeek.OrderBy(d => d).SequenceEqual(other.DaysOfWeek.OrderBy(d => d)) &&
               _yearsMonths.OrderBy(k => k.Key)
                         .SequenceEqual(other._yearsMonths.OrderBy(k => k.Key));
    }

    public override bool Equals(object? obj) => Equals(obj as AgendaSeason);

    public override int GetHashCode() => _hashCode;

    public List<DateTime> GetDates()
    {
        var dates = new List<DateTime>();

        if (_yearsMonths == null || !_yearsMonths.Any() || _daysOfWeek == null || !_daysOfWeek.Any())
            return dates;

        foreach (var pair in _yearsMonths)
        {
            if (!int.TryParse(pair.Key, out int year))
                continue;

            var months = pair.Value;
            foreach (var monthEnum in months)
            {
                int month = (int)monthEnum + 1;
                int daysInMonth = DateTime.DaysInMonth(year, month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var currentDate = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);

                    if (_daysOfWeek.Contains((Weekday)currentDate.DayOfWeek))
                    {
                        dates.Add(currentDate);
                    }
                }

            }
        }

        return dates.OrderBy(d => d).ToList();
    }
}