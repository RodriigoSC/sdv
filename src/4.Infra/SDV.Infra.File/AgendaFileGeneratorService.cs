using System.Globalization;
using System.Text;
using SDV.Domain.Entities.Agendas;
using SDV.Domain.Entities.Calendars.ValueObjects;
using SDV.Domain.Entities.Messages.ValueObjects;
using SDV.Domain.Enums.Agendas;
using SDV.Domain.Enums.Commons;
using SDV.Domain.Extensions;
using SDV.Domain.Interfaces.Calendars;
using SDV.Domain.Interfaces.Messages;
using SDV.Infra.File.Model;

namespace SDV.Infra.File;

public class AgendaFileGeneratorService : IAgendaFileGeneratorService
{
    private readonly ICalendarService _calendarService;
    private readonly IMessageService _messageService;

    public AgendaFileGeneratorService(ICalendarService calendarService, IMessageService messageService)
    {
        _calendarService = calendarService;
        _messageService = messageService;
    }

    public async Task<List<AgendaFile>> GenerateAgendaDataAsync(Agenda agenda)
    {
        var culture = new CultureInfo(agenda.Configuration.Culture);
        var dates = agenda.Season.GetDates();
        var result = new List<AgendaFile>();

        // Carrega calendários e mensagens
        var calendarsTask = _calendarService.GetCalendarByIdAsync(agenda.CalendarTemplateId ?? Guid.Empty);
        var messagesTask = _messageService.GetMessageByIdAsync(agenda.MessageTemplateId ?? Guid.Empty);
        await Task.WhenAll(calendarsTask, messagesTask);

        var calendarResult = await calendarsTask;
        var messageResult = await messagesTask;
        
        IEnumerable<CalendarDay> calendars = calendarResult.IsSuccess && calendarResult.Value != null ? calendarResult.Value.Calendars : Enumerable.Empty<CalendarDay>();
        IEnumerable<MessageDay> messages = messageResult.IsSuccess && messageResult.Value != null ? messageResult.Value.Messages : Enumerable.Empty<MessageDay>();

        // Processa cada data
        foreach (var date in dates)
        {
            var dayOfWeek = agenda.Configuration.WeekAbbreviation == WeekAbbreviation.Short
                ? culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek).ToTitleCase()
                : culture.DateTimeFormat.GetDayName(date.DayOfWeek).ToTitleCase();

            string formattedDay = agenda.Configuration.DayNumberFormat == DayNumberFormat.DoubleDigit
                ? date.ToString("dd/MM/yyyy")
                : date.ToString("d/M/yyyy");

            string? calendarName = calendars.FirstOrDefault(h => h.Date.Date == date.Date)?.Content;
            string? messageName  = messages.FirstOrDefault(h => h.Date.Date == date.Date)?.Content;


            result.Add(new AgendaFile
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

    public byte[] GenerateAgendaCsv(Agenda agenda, IEnumerable<AgendaFile> data)
    {
        var culture = new CultureInfo(agenda.Configuration.Culture);
        var sb = new StringBuilder();
        var dataList = data.ToList();

        int blockSize = GetBlockSize(agenda.AgendaType);
        bool includeCalendar = agenda.CalendarTemplateId.HasValue;
        bool includeMessage = agenda.MessageTemplateId.HasValue;

        BuildCsvHeader(sb, blockSize, includeCalendar, includeMessage);

        var rowData = new List<string>();
        int dayInRowCounter = 0;

        if (agenda.AgendaType == AgendaType.Weekly && dataList.Any())
        {
            dayInRowCounter = AddWeeklyStartPadding(rowData,dataList[0].Date,agenda.Configuration.StartOfWeek,includeCalendar,includeMessage);
        }

        foreach (var dayData in dataList)
        {
            AddDayToRow(rowData, agenda, dayData, culture, includeCalendar, includeMessage);
            dayInRowCounter++;

            bool isLastDayOfMonth = dayData.Date.Day == DateTime.DaysInMonth(dayData.Date.Year, dayData.Date.Month);
            bool shouldWriteRow = dayInRowCounter == blockSize || (agenda.AgendaType != AgendaType.Weekly && isLastDayOfMonth);

            if (shouldWriteRow)
            {
                AddEndOfRowPadding(rowData, blockSize, dayInRowCounter, agenda.AgendaType, includeCalendar, includeMessage);
                
                sb.AppendLine(string.Join(";", rowData));
                rowData.Clear();
                dayInRowCounter = 0;
            }
        }

        if (rowData.Any())
        {
            AddEndOfRowPadding(rowData, blockSize, dayInRowCounter, agenda.AgendaType, includeCalendar, includeMessage);
            sb.AppendLine(string.Join(";", rowData));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    #region Private Helpers

    private static int GetBlockSize(AgendaType agendaType) => agendaType switch
    {
        AgendaType.OneDayPerPage => 2,
        AgendaType.TwoDaysPerPage => 4,
        AgendaType.Weekly => 7,
        _ => 1
    };

    private static void BuildCsvHeader(StringBuilder sb, int blockSize, bool includeCalendar, bool includeMessage)
    {
        var headers = new List<string>();
        for (int i = 1; i <= blockSize; i++)
        {
            headers.Add($"DIA {i}");
            headers.Add($"SEMANA {i}");
            headers.Add($"MÊS {i}");
            if (includeCalendar) headers.Add($"FERIADO {i}");
            if (includeMessage) headers.Add($"MENSAGEM {i}");
        }
        sb.AppendLine(string.Join(";", headers));
    }
    
    private static int AddWeeklyStartPadding(List<string> rowData, DateTime firstDate, DayOfWeek startOfWeek, bool includeCalendar, bool includeMessage)
    {
        int paddingDays = ((int)firstDate.DayOfWeek - (int)startOfWeek + 7) % 7;
        for (int i = 0; i < paddingDays; i++)
        {
            AddPaddingBlock(rowData, includeCalendar, includeMessage, "");
        }
        return paddingDays;
    }

    private static void AddDayToRow(List<string> rowData, Agenda agenda, AgendaFile dayData, CultureInfo culture, bool includeCalendar, bool includeMessage)
    {
        string formattedDay = agenda.Configuration.DayNumberFormat == DayNumberFormat.DoubleDigit
            ? dayData.Date.Day.ToString("00")
            : dayData.Date.Day.ToString();
        
        string monthName = agenda.Configuration.MonthAbbreviation == MonthAbbreviation.Short
            ? culture.DateTimeFormat.GetAbbreviatedMonthName(dayData.Date.Month).ToTitleCase()
            : culture.DateTimeFormat.GetMonthName(dayData.Date.Month).ToTitleCase();

        rowData.Add(formattedDay);
        rowData.Add(dayData.DayOfWeek ?? "");
        rowData.Add(monthName);
        if (includeCalendar) rowData.Add(dayData.CalendarName ?? "");
        if (includeMessage) rowData.Add(dayData.MessageName ?? "");
    }
    
    private static void AddPaddingBlock(List<string> rowData, bool includeCalendar, bool includeMessage, string monthValue = "Notas")
    {
        rowData.Add("");
        rowData.Add("");
        rowData.Add(monthValue);
        if (includeCalendar) rowData.Add("");
        if (includeMessage) rowData.Add("");
    }

    private static void AddEndOfRowPadding(List<string> rowData, int blockSize, int dayInRowCounter, AgendaType agendaType, bool includeCalendar, bool includeMessage)
    {
        if (agendaType != AgendaType.Weekly && dayInRowCounter < blockSize)
        {
            int remainingBlocks = blockSize - dayInRowCounter;
            for (int j = 0; j < remainingBlocks; j++)
            {
                AddPaddingBlock(rowData, includeCalendar, includeMessage);
            }
        }
    }
    #endregion
}