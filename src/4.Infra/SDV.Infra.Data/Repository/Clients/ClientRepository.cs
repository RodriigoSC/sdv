using MongoDB.Driver;
using SDV.Domain.Entities.Clients;
using SDV.Domain.Interfaces.Clients;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Repository.Common;

namespace SDV.Infra.Data.Repository.Clients;

public class ClientRepository : CommonRepository, IClientRepository
{
    private readonly IMongoCollection<Client> _collection;

    public ClientRepository(IMongoDBRepository mongoDbRepository) : base(mongoDbRepository)
    {
        _collection = mongoDbRepository.GetCollection<Client>("Clients");

        // Cria índice se necessário
        MongoDbVerifyAndCreateIndex("Clients", new List<string> { "Email" });
    }

    public async Task AddAsync(Client entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Client>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter);
    }

    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Client entity)
    {
        var filter = Builders<Client>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity);
    }
}
