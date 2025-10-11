using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Vault;


namespace SDV.Tests.Api
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        public Action<IServiceCollection>? TestServices { get; set; }

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
                services.RemoveAll<IMongoDBRepository>();

                var vaultMock = new Mock<IVaultService>();
                vaultMock.Setup(v => v.GetKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync("mock_value");
                services.AddSingleton(vaultMock.Object);

                if (TestServices != null)
                {
                    TestServices(services);
                }
                else
                {
                    var mongoRepositoryMock = new Mock<IMongoDBRepository>();
                    services.AddSingleton(mongoRepositoryMock.Object);
                }
            });
        }
    }
}