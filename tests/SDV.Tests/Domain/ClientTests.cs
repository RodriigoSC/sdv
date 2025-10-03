using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Clients.ValueObjects;

namespace SDV.Tests.Domain
{
    public class ClientTests
    {
        [Fact]
        public void Should_Create_Client_With_Valid_Data()
        {
            var client = new Client("John Doe", Email.Create("john.doe@example.com"), "password123");
            Assert.NotNull(client);
            Assert.Equal("John Doe", client.Name);
            Assert.True(client.IsActive);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_Throw_Exception_For_Invalid_Name(string name)
        {
            Assert.Throws<ArgumentException>(() => new Client(name, Email.Create("john.doe@example.com"), "password123"));
        }

        [Fact]
        public void Should_Change_Name_Successfully()
        {
            var client = new Client("Old Name", Email.Create("test@test.com"), "password");
            client.ChangeName("New Name");
            Assert.Equal("New Name", client.Name);
        }
    }
}