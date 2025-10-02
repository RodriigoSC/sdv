using System;
using SDV.Domain.Entities.Agendas;
using SDV.Infra.File.Model;

namespace SDV.Infra.File;

public interface IAgendaFileGeneratorService
{
    Task<List<AgendaFile>> GenerateAgendaDataAsync(Agenda agenda);
    byte[] GenerateAgendaCsv(Agenda agenda, IEnumerable<AgendaFile> data);
}
