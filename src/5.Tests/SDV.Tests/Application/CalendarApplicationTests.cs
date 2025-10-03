using Moq;
using SDV.Application.Services;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Interfaces.Calendars;
namespace SDV.Tests.Application
{
    public class CalendarApplicationTests
    {
        private readonly Mock<ICalendarService> _calendarServiceMock;
        private readonly CalendarApplication _sut;

        public CalendarApplicationTests()
        {
            _calendarServiceMock = new Mock<ICalendarService>();
            _sut = new CalendarApplication(_calendarServiceMock.Object);
        }

        [Fact]
        public async Task DeleteCalendar_WithValidId_ShouldReturnSuccess()
        {
            var calendarId = Guid.NewGuid();
            _calendarServiceMock.Setup(s => s.DeleteCalendarAsync(calendarId)).ReturnsAsync(Result<bool>.Success(true));

            var result = await _sut.DeleteCalendar(calendarId.ToString());

            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
        }
    }
}