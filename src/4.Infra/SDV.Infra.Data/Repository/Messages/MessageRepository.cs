using MongoDB.Driver;
using SDV.Domain.Entities.Messages;
using SDV.Domain.Interfaces.Messages;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Repository.Common;

namespace SDV.Infra.Data.Repository.Messages;

public class MessageRepository : CommonRepository, IMessageRepository
{
    private readonly IMongoCollection<Message> _collection;

    public MessageRepository(IMongoDBRepository mongoDbRepository) : base(mongoDbRepository)
    {
        _collection = mongoDbRepository.GetCollection<Message>("Messages");

        MongoDbVerifyAndCreateIndex("Messages", new List<string> { "ClientId" });
    }

    public async Task AddAsync(Message entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Message>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter);
    }

    public async Task<IEnumerable<Message>> GetAllAsync(Guid clientId)
    {
        return await _collection.Find(x => x.ClientId == clientId).ToListAsync();
    }

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Message entity)
    {
        var filter = Builders<Message>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity);
    }

}
