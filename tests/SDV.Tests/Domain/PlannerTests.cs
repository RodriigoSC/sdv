using SDV.Domain.Entities.Planners;
using SDV.Domain.Enums.Planners;


namespace SDV.Tests.Domain
{
    public class PlannerTests
    {
        private readonly Guid _clientId = Guid.NewGuid();

        [Fact]
        public void Should_Create_Planner_Successfully()
        {
            var planner = new Planner(_clientId, "My Planner", PlannerType.Weekly);
            Assert.NotNull(planner);
            Assert.Equal(_clientId, planner.ClientId);
            Assert.Equal("My Planner", planner.Title);
        }
    }
}