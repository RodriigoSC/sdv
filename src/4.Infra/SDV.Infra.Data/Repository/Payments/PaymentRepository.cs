

using MongoDB.Driver;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Interfaces.Payments;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Repository.Common;

namespace SDV.Infra.Data.Repository.Payments;

public class PaymentRepository : CommonRepository, IPaymentRepository
{
    private readonly IMongoCollection<Payment> _collection;

    public PaymentRepository(IMongoDBRepository mongoDbRepository) : base(mongoDbRepository)
    {
        _collection = mongoDbRepository.GetCollection<Payment>("Payments");

        MongoDbVerifyAndCreateIndex("Payments", new List<string> { "ClientId", "TransactionId" });
    }

    public async Task AddAsync(Payment entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Payment>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
    {
        var filter = Builders<Payment>.Filter.Eq(p => p.TransactionId, transactionId);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByClientIdAsync(Guid clientId)
    {
        var filter = Builders<Payment>.Filter.Eq(p => p.ClientId, clientId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task UpdateAsync(Payment entity)
    {
        var filter = Builders<Payment>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity);
    }
}
