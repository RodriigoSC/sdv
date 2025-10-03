using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Moq;
using SDV.Domain.Entities.Agendas;
using SDV.Domain.Entities.Calendars;
using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Messages;
using SDV.Domain.Entities.Planners;
using SDV.Infra.Consul;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Vault;

namespace SDV.Tests.Api
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ASPNETCORE_ENVIRONMENT"] = "Testing",
                    ["CONN_STRING_VAULT"] = "http://dummy-vault",
                    ["USER_VAULT"] = "dummy-user",
                    ["PASS_VAULT"] = "dummy-pass"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IVaultService>();
                services.RemoveAll<IConsulService>();
                services.RemoveAll<IMongoDBRepository>();

                var vaultMock = new Mock<IVaultService>();
                vaultMock.Setup(v => v.GetKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync("mock_value");
                services.AddSingleton(vaultMock.Object);

                var consulMock = new Mock<IConsulService>();
                services.AddSingleton(consulMock.Object);

                // --- Configuração completa do Mock do MongoDB ---
                var mongoRepositoryMock = new Mock<IMongoDBRepository>();

                // Mock para a coleção de Clients
                mongoRepositoryMock.Setup(r => r.GetCollection<Client>("Clients"))
                                   .Returns(GetMockedMongoCollection<Client>().Object);

                // Mock para a coleção de Agendas
                mongoRepositoryMock.Setup(r => r.GetCollection<Agenda>("Agendas"))
                                   .Returns(GetMockedMongoCollection<Agenda>().Object);

                // Mock para a coleção de Planners
                mongoRepositoryMock.Setup(r => r.GetCollection<Planner>("Planners"))
                                   .Returns(GetMockedMongoCollection<Planner>().Object);
                                   
                // Mock para a coleção de Calendars
                mongoRepositoryMock.Setup(r => r.GetCollection<Calendar>("Calendars"))
                                   .Returns(GetMockedMongoCollection<Calendar>().Object);

                // Mock para a coleção de Messages
                mongoRepositoryMock.Setup(r => r.GetCollection<Message>("Messages"))
                                   .Returns(GetMockedMongoCollection<Message>().Object);

                services.AddSingleton(mongoRepositoryMock.Object);
            });
        }

        /// <summary>
        /// Helper para criar um mock de IMongoCollection<T> que simula um banco de dados vazio.
        /// </summary>
        private static Mock<IMongoCollection<T>> GetMockedMongoCollection<T>()
        {
            var collectionMock = new Mock<IMongoCollection<T>>();
            var cursorMock = new Mock<IAsyncCursor<T>>();
            var emptyList = new List<T>();

            cursorMock.Setup(_ => _.Current).Returns(emptyList);
            cursorMock.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            cursorMock.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            collectionMock.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<T>>(),
                It.IsAny<FindOptions<T, T>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);
            
            return collectionMock;
        }
    }
}

