using SDV.Domain.Entities.Calendars;
using SDV.Domain.Entities.Calendars.ValueObjects;

namespace SDV.Tests.Domain
{
    public class CalendarTests
    {
        private readonly Guid _clientId = Guid.NewGuid();

        [Fact]
        public void Should_Create_Calendar_Successfully()
        {
            var calendar = new Calendar(_clientId, "Holidays");
            Assert.NotNull(calendar);
            Assert.Empty(calendar.Calendars);
        }

        [Fact]
        public void Should_Add_CalendarDay_Successfully()
        {
            var calendar = new Calendar(_clientId, "Holidays");
            var holiday = new CalendarDay("New Year", new DateTime(2025, 1, 1));
            calendar.AddCalendar(holiday);
            Assert.Single(calendar.Calendars);
        }
    }
}