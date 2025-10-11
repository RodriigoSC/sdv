using MongoDB.Driver;
using SDV.Domain.Entities.Planners;
using SDV.Domain.Interfaces.Planners;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Repository.Common;

namespace SDV.Infra.Data.Repository.Planners;

public class PlannerRepository : CommonRepository, IPlannerRepository
{
    private readonly IMongoCollection<Planner> _collection;
    
    public PlannerRepository(IMongoDBRepository mongoDbRepository) : base(mongoDbRepository)
    {
        _collection = mongoDbRepository.GetCollection<Planner>("Planners");

        MongoDbVerifyAndCreateIndex("Planners", new List<string> { "ClientId" });
    }
    
    public async Task AddAsync(Planner entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Planner>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter);
    }

    public async Task<IEnumerable<Planner>> GetAllAsync(Guid clientId)
    {
        return await _collection.Find(x => x.ClientId == clientId).ToListAsync();
    }

    public async Task<Planner?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Planner entity)
    {
        var filter = Builders<Planner>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity);
    }
}
