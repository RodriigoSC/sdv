using SDV.Domain.Entities.Messages;
using SDV.Domain.Entities.Messages.ValueObjects;

namespace SDV.Tests.Domain
{
    public class MessageTests
    {
        private readonly Guid _clientId = Guid.NewGuid();

        [Fact]
        public void Should_Create_Message_Successfully()
        {
            var message = new Message(_clientId, "Greetings");
            Assert.NotNull(message);
            Assert.Empty(message.Messages);
        }

        [Fact]
        public void Should_Add_MessageDay_Successfully()
        {
            var message = new Message(_clientId, "Greetings");
            var msgDay = new MessageDay("Happy Birthday!", new DateTime(2025, 10, 3));
            message.AddMessage(msgDay);
            Assert.Single(message.Messages);
        }
    }
}