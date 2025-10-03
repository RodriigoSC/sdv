using MongoDB.Bson.Serialization.Attributes;
using SDV.Domain.Enums.Commons;

namespace SDV.Domain.Entities.Planners.ValueObjects;

public sealed class PlannerConfiguration : IEquatable<PlannerConfiguration>
{
    [BsonElement("DayNumberFormat")]
    public DayNumberFormat DayNumberFormat { get; }

    [BsonElement("WeekAbbreviation")]
    public WeekAbbreviation WeekAbbreviation { get; }

    [BsonElement("MonthAbbreviation")]
    public MonthAbbreviation MonthAbbreviation { get; }

    [BsonElement("FileType")]
    public FileType FileType { get; }

    [BsonElement("Culture")]
    public string Culture { get; }

    [BsonElement("StartOfWeek")]
    public DayOfWeek StartOfWeek { get; }

    [BsonConstructor]
    public PlannerConfiguration(DayNumberFormat dayNumberFormat, WeekAbbreviation weekAbbreviation, MonthAbbreviation monthAbbreviation, FileType fileType, string culture, DayOfWeek startOfWeek)
    {
        DayNumberFormat = dayNumberFormat;
        WeekAbbreviation = weekAbbreviation;
        MonthAbbreviation = monthAbbreviation;
        FileType = fileType;
        Culture = culture;
        StartOfWeek = startOfWeek;
    }

    public static PlannerConfiguration Default() =>
        new PlannerConfiguration(DayNumberFormat.DoubleDigit, WeekAbbreviation.Short, MonthAbbreviation.Short, FileType.CSV, "pt-BR", DayOfWeek.Monday);

    public bool Equals(PlannerConfiguration? other)
    {
        if (other is null) return false;
        return DayNumberFormat == other.DayNumberFormat &&
                WeekAbbreviation == other.WeekAbbreviation &&
                MonthAbbreviation == other.MonthAbbreviation &&
                FileType == other.FileType &&
                Culture == other.Culture &&
                StartOfWeek == other.StartOfWeek;
    }

    public override bool Equals(object? obj) => Equals(obj as PlannerConfiguration);

    public override int GetHashCode() =>
        HashCode.Combine(DayNumberFormat, WeekAbbreviation, MonthAbbreviation, FileType);
}