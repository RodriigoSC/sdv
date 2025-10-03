using System.Globalization;
using System.Text;
using SDV.Domain.Entities.Calendars.ValueObjects;
using SDV.Domain.Entities.Messages.ValueObjects;
using SDV.Domain.Entities.Planners;
using SDV.Domain.Entities.Planners.ValueObjects;
using SDV.Domain.Enums.Commons;
using SDV.Domain.Extensions;
using SDV.Domain.Interfaces.Calendars;
using SDV.Domain.Interfaces.Messages;
using SDV.Infra.File.Model;

namespace SDV.Infra.File;

public class PlannerFileGeneratorService : IPlannerFileGeneratorService
{
    private readonly ICalendarService _calendarService;
    private readonly IMessageService _messageService;

    public PlannerFileGeneratorService(ICalendarService calendarService, IMessageService messageService)
    {
        _calendarService = calendarService;
        _messageService = messageService;
    }

    // Nenhuma alteração necessária neste método.
    public async Task<List<PlannerFile>> GeneratePlannerDataAsync(Planner planner)
    {
        var culture = new CultureInfo(planner.Configuration.Culture);
        var result = new List<PlannerFile>();
        var dates = planner.Season.GetDates();

        var calendars = Enumerable.Empty<CalendarDay>();
        if (planner.CalendarTemplateId.HasValue)
        {
            var calendarResult = await _calendarService.GetCalendarByIdAsync(planner.CalendarTemplateId.Value);
            if (calendarResult.IsSuccess && calendarResult.Value != null)
                calendars = calendarResult.Value.Calendars;
        }

        var messages = Enumerable.Empty<MessageDay>();
        if (planner.MessageTemplateId.HasValue)
        {
            var messageResult = await _messageService.GetMessageByIdAsync(planner.MessageTemplateId.Value);
            if (messageResult.IsSuccess && messageResult.Value != null)
                messages = messageResult.Value.Messages;
        }

        foreach (var date in dates)
        {
            var dayOfWeek = planner.Configuration.WeekAbbreviation == WeekAbbreviation.Short
                ? culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek).ToTitleCase()
                : culture.DateTimeFormat.GetDayName(date.DayOfWeek).ToTitleCase();

            string formattedDay = planner.Configuration.DayNumberFormat == DayNumberFormat.DoubleDigit
                        ? date.ToString("dd/MM/yyyy")
                        : date.ToString("d/M/yyyy");
                        
            string? calendarName = calendars.FirstOrDefault(h => h.Date.Date == date.Date)?.Content;
            string? messageName  = messages.FirstOrDefault(h => h.Date.Date == date.Date)?.Content;

            result.Add(new PlannerFile
            {
                Date = date,
                DayOfWeek = dayOfWeek,
                FormattedDayAndMonth = formattedDay,
                CalendarName = calendarName,
                MessageName = messageName
            });
        }
        return result;
    }

    public byte[] GeneratePlannerCsv(Planner planner, IEnumerable<PlannerFile> data)
    {
        var dataList = data.ToList();
        if (!dataList.Any())
        {
            return Array.Empty<byte>();
        }

        var culture = new CultureInfo(planner.Configuration.Culture);
        var sb = new StringBuilder();
        var startOfWeek = planner.Configuration.StartOfWeek;

        var orderedSelectedDays = planner.Season.DaysOfWeek
            .Select(d => (DayOfWeek)d)
            .OrderBy(d => (d - startOfWeek + 7) % 7)
            .ToList();

        if (!orderedSelectedDays.Any())
        {
            return Array.Empty<byte>();
        }

        int blockSize = orderedSelectedDays.Count;
        bool includeCalendar = planner.CalendarTemplateId.HasValue;
        bool includeMessage = planner.MessageTemplateId.HasValue;

        var dayToColumnIndexMap = orderedSelectedDays
            .Select((day, index) => new { Day = day, Index = index })
            .ToDictionary(x => x.Day, x => x.Index);

        BuildCsvHeader(sb, blockSize, includeCalendar, includeMessage);

        var rowData = new List<string>();
        PlannerFile? previousData = null;

        foreach (var currentData in dataList)
        {
            if (previousData != null)
            {
                // ** LÓGICA CORRIGIDA AQUI **
                bool isNewWeek = false;
                if (dayToColumnIndexMap.TryGetValue(currentData.Date.DayOfWeek, out var currentIndex) &&
                    dayToColumnIndexMap.TryGetValue(previousData.Date.DayOfWeek, out var previousIndex))
                {
                    // A semana "vira" se o dia atual está numa coluna anterior ou na mesma que o dia anterior
                    isNewWeek = currentIndex <= previousIndex;
                }

                // Verifica se o mês mudou
                bool isNewMonth = currentData.Date.Month != previousData.Date.Month;

                // Se for uma nova semana ou um novo mês, finalizamos a linha atual.
                if (isNewWeek || isNewMonth)
                {
                    FinishAndAppendLine(sb, rowData, blockSize, includeCalendar, includeMessage);
                }
            }
            
            AddPadding(rowData, currentData, dayToColumnIndexMap, includeCalendar, includeMessage);
            AddDayDataToRow(rowData, currentData, planner.Configuration, culture, includeCalendar, includeMessage);
            previousData = currentData;
        }

        if (rowData.Any())
        {
            FinishAndAppendLine(sb, rowData, blockSize, includeCalendar, includeMessage);
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    #region Private Helpers

    private static void AddDayDataToRow(List<string> rowData, PlannerFile d, PlannerConfiguration config, CultureInfo culture, bool includeCalendar, bool includeMessage)
    {
        string diaFormatado = config.DayNumberFormat == DayNumberFormat.DoubleDigit ? d.Date.Day.ToString("00") : d.Date.Day.ToString();
        string monthName = config.MonthAbbreviation == MonthAbbreviation.Short
            ? culture.DateTimeFormat.GetAbbreviatedMonthName(d.Date.Month).ToTitleCase()
            : culture.DateTimeFormat.GetMonthName(d.Date.Month).ToTitleCase();
        
        rowData.Add(diaFormatado);
        rowData.Add(d.DayOfWeek ?? "");
        rowData.Add(monthName);
        if (includeCalendar) rowData.Add(d.CalendarName ?? "");
        if (includeMessage) rowData.Add(d.MessageName ?? "");
    }

    private static void BuildCsvHeader(StringBuilder sb, int blockSize, bool includeCalendar, bool includeMessage)
    {
        var columns = new List<string>();
        for (int i = 1; i <= blockSize; i++)
        {
            var columnParts = new List<string> { $"DIA {i}", $"SEMANA {i}", $"MÊS {i}" };
            if (includeCalendar) columnParts.Add($"FERIADO {i}");
            if (includeMessage) columnParts.Add($"MENSAGEM {i}");
            columns.Add(string.Join(";", columnParts));
        }
        sb.AppendLine(string.Join(";", columns));
    }

    private static void AddEmptyBlock(List<string> rowData, bool includeCalendar, bool includeMessage)
    {
        rowData.AddRange(new[] { "", "", "" });
        if (includeCalendar) rowData.Add("");
        if (includeMessage) rowData.Add("");
    }
    
    private static int GetFieldsPerBlock(bool includeCalendar, bool includeMessage)
    {
        return 3 + (includeCalendar ? 1 : 0) + (includeMessage ? 1 : 0);
    }

    private static void FinishAndAppendLine(StringBuilder sb, List<string> rowData, int blockSize, bool includeCalendar, bool includeMessage)
    {
        int fieldsPerBlock = GetFieldsPerBlock(includeCalendar, includeMessage);
        while (rowData.Count < blockSize * fieldsPerBlock)
        {
            rowData.Add(""); 
        }
        sb.AppendLine(string.Join(";", rowData));
        rowData.Clear();
    }

    private static void AddPadding(List<string> rowData, PlannerFile currentData, Dictionary<DayOfWeek, int> dayToColumnIndexMap, bool includeCalendar, bool includeMessage)
    {
        if (!dayToColumnIndexMap.TryGetValue(currentData.Date.DayOfWeek, out int targetColumnIndex))
        {
            return;
        }

        int fieldsPerBlock = GetFieldsPerBlock(includeCalendar, includeMessage);
        int currentColumnCount = rowData.Count / fieldsPerBlock;

        for (int i = currentColumnCount; i < targetColumnIndex; i++)
        {
            AddEmptyBlock(rowData, includeCalendar, includeMessage);
        }
    }

    #endregion
}