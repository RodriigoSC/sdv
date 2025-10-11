using Moq;
using SDV.Application.Services;
using SDV.Domain.Interfaces.Planners;


namespace SDV.Tests.Application
{
    public class PlannerApplicationTests
    {
        private readonly Mock<IPlannerService> _plannerServiceMock;
        private readonly PlannerApplication _sut;

        public PlannerApplicationTests()
        {
            _plannerServiceMock = new Mock<IPlannerService>();
            _sut = new PlannerApplication(_plannerServiceMock.Object);
        }

        [Fact]
        public async Task GetPlannerById_WithInvalidIdFormat_ShouldReturnFailure()
        {
            var result = await _sut.GetPlannerById("invalid-guid");

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.OperationCode);
        }
    }
}