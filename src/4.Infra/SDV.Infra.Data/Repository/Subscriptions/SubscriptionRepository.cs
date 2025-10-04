using System;
using MongoDB.Driver;
using SDV.Domain.Entities.Subscriptions;
using SDV.Domain.Interfaces.Subscriptions;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Repository.Common;

namespace SDV.Infra.Data.Repository.Subscriptions;

public class SubscriptionRepository : CommonRepository, ISubscriptionRepository
{
    private readonly IMongoCollection<Subscription> _collection;

    public SubscriptionRepository(IMongoDBRepository mongoDbRepository) : base(mongoDbRepository)
    {
        _collection = mongoDbRepository.GetCollection<Subscription>("Subscriptions");

        MongoDbVerifyAndCreateIndex("Subscriptions", new List<string> { "ClientId" });
    }

    public async Task AddAsync(Subscription entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Subscription>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter);
    }

    public async Task<IEnumerable<Subscription>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Subscription?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Subscription entity)
    {
        var filter = Builders<Subscription>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity);
    }

}
