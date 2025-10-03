using Moq;
using SDV.Application.Services;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Interfaces.Messages;

namespace SDV.Tests.Application
{
    public class MessageApplicationTests
    {
        private readonly Mock<IMessageService> _messageServiceMock;
        private readonly MessageApplication _sut;

        public MessageApplicationTests()
        {
            _messageServiceMock = new Mock<IMessageService>();
            _sut = new MessageApplication(_messageServiceMock.Object);
        }

        [Fact]
        public async Task DeleteMessage_WhenServiceFails_ShouldReturnFailure()
        {
            var messageId = Guid.NewGuid();
            _messageServiceMock.Setup(s => s.DeleteMessageAsync(messageId)).ReturnsAsync(Result<bool>.Failure("Not found"));

            var result = await _sut.DeleteMessage(messageId.ToString());

            Assert.False(result.IsSuccess);
            Assert.False(result.Data);
            Assert.Equal(400, result.OperationCode);
        }
    }
}