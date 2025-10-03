using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SDV.Application.Dtos.Agendas;
using SDV.Application.Interfaces;
using SDV.Application.Results;
using Xunit;

namespace SDV.Tests.Api
{
    public class AgendaControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AgendaControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAllByClient_WhenNoAgendasExist_ShouldReturnNoContent()
        {
            // Arrange
            var clientId = Guid.NewGuid();

            // Cria uma factory específica para este teste, com o serviço mockado
            var factoryWithMock = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove o registro original da IAgendaApplication
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAgendaApplication));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Cria o mock e configura o método GetAllAgendas
                    var agendaAppMock = new Mock<IAgendaApplication>();
                    agendaAppMock
                        .Setup(app => app.GetAllAgendas(clientId.ToString()))
                        .ReturnsAsync(OperationResult<IEnumerable<AgendaDto>>.Succeeded(Enumerable.Empty<AgendaDto>()));

                    // Adiciona o mock ao contêiner de DI
                    services.AddSingleton(agendaAppMock.Object);
                });
            });

            // Cria um cliente HTTP a partir da factory com o mock
            var httpClient = factoryWithMock.CreateClient();

            // Act
            var response = await httpClient.GetAsync($"/api/agenda/client/{clientId}");

            // Assert
            // Agora o teste deve passar, pois o mock retorna uma lista vazia,
            // e o controller deve corretamente traduzir isso para um status 204 NoContent.
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}