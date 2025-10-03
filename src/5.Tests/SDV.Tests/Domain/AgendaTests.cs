using SDV.Domain.Entities.Agendas;
using SDV.Domain.Enums.Agendas;

namespace SDV.Tests.Domain
{
    public class AgendaTests
    {
        private readonly Guid _clientId = Guid.NewGuid();

        [Fact]
        public void Should_Create_Agenda_With_Default_Configuration()
        {
            var agenda = new Agenda(_clientId, "My Agenda", AgendaType.Weekly);
            Assert.NotNull(agenda);
            Assert.Equal("My Agenda", agenda.Title);
            Assert.NotNull(agenda.Configuration);
            Assert.NotNull(agenda.Season);
        }

        [Fact]
        public void Should_Change_Title_Successfully()
        {
            var agenda = new Agenda(_clientId, "Old Title", AgendaType.OneDayPerPage);
            agenda.ChangeTitle("New Title");
            Assert.Equal("New Title", agenda.Title);
        }

        [Fact]
        public void Should_Throw_Exception_When_Changing_Title_To_Empty()
        {
            var agenda = new Agenda(_clientId, "Title", AgendaType.OneDayPerPage);
            Assert.Throws<ArgumentException>(() => agenda.ChangeTitle(""));
        }
    }
}