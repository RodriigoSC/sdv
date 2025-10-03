using System.Net;


namespace SDV.Tests.Api
{
    public class ClientControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        // Construtor Estático: Executado uma vez, antes de tudo.
        static ClientControllerTests()
        {
            // Define variáveis de ambiente "falsas" para o processo de teste.
            // Isso satisfaz a lógica de inicialização no Bootstrap.cs, permitindo que a API inicie.
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("CONN_STRING_VAULT", "http://dummy-vault:8200");
            Environment.SetEnvironmentVariable("USER_VAULT", "dummy-user");
            Environment.SetEnvironmentVariable("PASS_VAULT", "dummy-pass");
        }

        public ClientControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_Clients_ShouldReturnNoContent_WhenDbIsEmpty()
        {
            // Act
            var response = await _client.GetAsync("/api/client");
            
            // Assert
            // Como o banco de dados está mockado e vazio, o endpoint deve retornar NoContent.
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}

