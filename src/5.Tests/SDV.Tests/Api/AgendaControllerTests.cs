using System.Net;


namespace SDV.Tests.Api
{
    // Atualizamos para usar nossa CustomWebApplicationFactory
    public class AgendaControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AgendaControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllByClient_WithAnyClientId_ShouldReturnNoContentOrOk()
        {
            // Arrange
            var clientId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/agenda/client/{clientId}");

            // Assert
            // Como o banco de dados está mockado e vazio, o esperado é NoContent (204).
            // Se você mockasse dados de retorno, poderia esperar OK (200).
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
