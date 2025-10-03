using Moq;
using SDV.Application.Services;
using SDV.Domain.Entities.Agendas;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Enums.Agendas;
using SDV.Domain.Interfaces.Agendas;

namespace SDV.Tests.Application
{
    public class AgendaApplicationTests
    {
        private readonly Mock<IAgendaService> _agendaServiceMock;
        private readonly AgendaApplication _sut;

        public AgendaApplicationTests()
        {
            _agendaServiceMock = new Mock<IAgendaService>();
            _sut = new AgendaApplication(_agendaServiceMock.Object);
        }

        [Fact]
        public async Task GetAgendaById_WithValidId_ShouldReturnSuccess()
        {
            var agendaId = Guid.NewGuid();
            var agenda = new Agenda(Guid.NewGuid(), "Test Agenda", AgendaType.Weekly);
            _agendaServiceMock.Setup(s => s.GetAgendaByIdAsync(agendaId)).ReturnsAsync(Result<Agenda>.Success(agenda));

            var result = await _sut.GetAgendaById(agendaId.ToString());

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
        }
    }
}