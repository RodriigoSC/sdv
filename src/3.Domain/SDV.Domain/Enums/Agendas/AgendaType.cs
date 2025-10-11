using System.ComponentModel.DataAnnotations;

namespace SDV.Domain.Enums.Agendas;

/// <summary>
/// Representa os tipos de agenda disponíveis.
/// </summary>
public enum AgendaType
{
    /// <summary>
    /// A agenda exibirá um dia por página.
    /// </summary>
    [Display(Name = "1 dia por página")]
    OneDayPerPage,

    /// <summary>
    /// A agenda exibirá dois dias por página.
    /// </summary>
    [Display(Name = "2 dias por página")]
    TwoDaysPerPage,

    /// <summary>
    /// A agenda será exibida em formato semanal.
    /// </summary>
    [Display(Name = "Agenda Semanal")]
    Weekly
}