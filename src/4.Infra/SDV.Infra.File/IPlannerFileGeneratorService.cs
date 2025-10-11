using System;
using SDV.Domain.Entities.Planners;
using SDV.Infra.File.Model;

namespace SDV.Infra.File;

public interface IPlannerFileGeneratorService
{
    Task<List<PlannerFile>> GeneratePlannerDataAsync(Planner planner);
    byte[] GeneratePlannerCsv(Planner planner, IEnumerable<PlannerFile> data);
}
