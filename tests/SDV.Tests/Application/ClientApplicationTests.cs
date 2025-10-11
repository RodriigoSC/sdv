using Moq;
using SDV.Application.Dtos.Clients;
using SDV.Application.Services;
using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Clients.ValueObjects;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Interfaces.Clients;

namespace SDV.Tests.Application
{
    public class ClientApplicationTests
    {
        private readonly Mock<IClientService> _clientServiceMock;
        private readonly ClientApplication _sut;

        public ClientApplicationTests()
        {
            _clientServiceMock = new Mock<IClientService>();
            _sut = new ClientApplication(_clientServiceMock.Object);
        }

        [Fact]
        public async Task GetAllClients_ShouldReturnSuccess()
        {
            var clients = new List<Client> { new Client("Test", Email.Create("test@test.com"), "pass") };
            _clientServiceMock.Setup(s => s.GetAllClientsAsync()).ReturnsAsync(Result<IEnumerable<Client>>.Success(clients));

            var result = await _sut.GetAllClients();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data); 
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task CreateClient_WithValidDto_ShouldReturnCreated()
        {
            var clientDto = new ClientDto { Name = "New", Email = "new@test.com", PasswordHash = "pass" };
            var client = new Client(clientDto.Name, Email.Create(clientDto.Email), clientDto.PasswordHash);
            _clientServiceMock.Setup(s => s.CreateClientAsync(It.IsAny<Client>())).ReturnsAsync(Result<Client>.Success(client));

            var result = await _sut.CreateClient(clientDto);

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.OperationCode);
        }
    }
}