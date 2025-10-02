using System.ComponentModel.DataAnnotations;

namespace SDV.Domain.Enums.Planners;

/// <summary>
/// Representa os tipos de planner disponíveis.
/// </summary>
public enum PlannerType
{
    /// <summary>
    /// Planner em formato semanal.
    /// </summary>
    [Display(Name = "Planner Semanal")]
    Weekly,
}