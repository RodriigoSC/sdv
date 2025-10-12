using MongoDB.Driver;
using SDV.Domain.Entities.Orders;
using SDV.Domain.Enums.Orders;
using SDV.Domain.Interfaces.Orders;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Repository.Common;

namespace SDV.Infra.Data.Repository.Orders;

public class OrderRepository : CommonRepository, IOrderRepository
{
    private readonly IMongoCollection<Order> _collection;

    public OrderRepository(IMongoDBRepository mongoDbRepository) : base(mongoDbRepository)
    {
        _collection = mongoDbRepository.GetCollection<Order>("Orders");

        MongoDbVerifyAndCreateIndex("Orders", new List<string> { "ClientId" });
    }

    public async Task AddAsync(Order entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter);
    }

    public async Task<Order?> GetActiveOrderByClientIdAsync(Guid clientId)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.ClientId, clientId) &
                    Builders<Order>.Filter.Eq(o => o.Status, OrderStatus.Active);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Order>> GetOrderHistoryByClientIdAsync(Guid clientId)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.ClientId, clientId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task UpdateAsync(Order entity)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity);
    }    
}
