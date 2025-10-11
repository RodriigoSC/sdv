using MongoDB.Driver;
using SDV.Domain.Entities.Agendas;
using SDV.Domain.Interfaces.Agendas;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Repository.Common;

namespace SDV.Infra.Data.Repository.Agendas;

public class AgendaRepository : CommonRepository, IAgendaRepository
{
    private readonly IMongoCollection<Agenda> _collection;

    public AgendaRepository(IMongoDBRepository mongoDbRepository) : base(mongoDbRepository)
    {
        _collection = mongoDbRepository.GetCollection<Agenda>("Agendas");

        MongoDbVerifyAndCreateIndex("Agendas", new List<string> { "ClientId" });
    }

    public async Task AddAsync(Agenda entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Agenda>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter);
    }

    public async Task<IEnumerable<Agenda>> GetAllAsync(Guid clientId)
    {
        return await _collection.Find(x => x.ClientId == clientId).ToListAsync();
    }

    public async Task<Agenda?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Agenda entity)
    {
        var filter = Builders<Agenda>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity);
    }
}
