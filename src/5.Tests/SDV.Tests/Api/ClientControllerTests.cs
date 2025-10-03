using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Moq;
using SDV.Api.Middlewares;
using SDV.Application.Dtos.Clients;
using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Clients.ValueObjects;
using SDV.Domain.Entities.Commons;
using SDV.Infra.Data.MongoDB;

namespace SDV.Tests.Api
{
    public class ClientControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ClientControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetOne_WithExistingId_ShouldReturnOk()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var clientEntity = new Client("Test Client", Email.Create("test@test.com"), "password");
            
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            if (idProperty != null && idProperty.CanWrite)
            {
                idProperty.SetValue(clientEntity, clientId, null);
            }

            var factoryWithData = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IMongoDBRepository));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    var mongoRepositoryMock = new Mock<IMongoDBRepository>();
                    var clients = new List<Client> { clientEntity };

                    SetupMongoCollectionMock(mongoRepositoryMock, "Clients", clients);
                    
                    services.AddSingleton(mongoRepositoryMock.Object);
                });
            });

            var httpClient = factoryWithData.CreateClient();

            // Act
            var response = await httpClient.GetAsync($"/api/client/{clientId}");
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ClientDto>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.IsSuccess);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(clientId.ToString(), apiResponse.Data.Id);
        }        

        private static void SetupMongoCollectionMock<T>(Mock<IMongoDBRepository> mongoMock, string collectionName, List<T> list)
        {
            var collectionMock = new Mock<IMongoCollection<T>>();
            var cursorMock = new Mock<IAsyncCursor<T>>();

            cursorMock.Setup(_ => _.Current).Returns(list);
            cursorMock.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            cursorMock.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            collectionMock.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<T>>(),
                It.IsAny<FindOptions<T, T>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);
            
            mongoMock.Setup(r => r.GetCollection<T>(collectionName)).Returns(collectionMock.Object);
        }
    }
}