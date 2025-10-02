namespace SDV.Domain.Enums;

/// <summary>
/// Representa os tipos de atividade do sistema para fins de log e auditoria.
/// </summary>
public enum ActivityType
{
    /// <summary>
    /// Uma nova agenda foi criada.
    /// </summary>
    AgendaCreated,

    /// <summary>
    /// Uma agenda existente foi atualizada.
    /// </summary>
    AgendaUpdated,

    /// <summary>
    /// Uma agenda foi excluída.
    /// </summary>
    AgendaDeleted,

    /// <summary>
    /// Uma agenda foi baixada.
    /// </summary>
    AgendaDownloaded,

    /// <summary>
    /// Um novo planner foi criado.
    /// </summary>
    PlannerCreated,

    /// <summary>
    /// Um planner existente foi atualizado.
    /// </summary>
    PlannerUpdated,

    /// <summary>
    /// Um planner foi excluído.
    /// </summary>
    PlannerDeleted,

    /// <summary>
    /// Um planner foi baixado.
    /// </summary>
    PlannerDownloaded,

    /// <summary>
    /// Um novo modelo de mensagem foi criado.
    /// </summary>
    MessageTemplateCreated,

    /// <summary>
    /// Um modelo de mensagem existente foi atualizado.
    /// </summary>
    MessageTemplateUpdated,

    /// <summary>
    /// Um modelo de mensagem foi excluído.
    /// </summary>
    MessageTemplateDeleted,

    /// <summary>
    /// Um novo modelo de calendário foi criado.
    /// </summary>
    CalendarTemplateCreated,

    /// <summary>
    /// Um modelo de calendário existente foi atualizado.
    /// </summary>
    CalendarTemplateUpdated,

    /// <summary>
    /// Um modelo de calendário foi excluído.
    /// </summary>
    CalendarTemplateDeleted
}