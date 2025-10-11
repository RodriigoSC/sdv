using System;
using MongoDB.Driver;
using SDV.Domain.Entities.Calendars;
using SDV.Domain.Interfaces.Calendars;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Repository.Common;

namespace SDV.Infra.Data.Repository.Calendars;

public class CalendarRepository : CommonRepository, ICalendarRepository
{
    private readonly IMongoCollection<Calendar> _collection;

    public CalendarRepository(IMongoDBRepository mongoDbRepository) : base(mongoDbRepository)
    {
        _collection = mongoDbRepository.GetCollection<Calendar>("Calendars");

        MongoDbVerifyAndCreateIndex("Calendars", new List<string> { "ClientId" });
    }

    public async Task AddAsync(Calendar entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Calendar>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter);
    }

    public async Task<IEnumerable<Calendar>> GetAllAsync(Guid clientId)
    {
        return await _collection.Find(x => x.ClientId == clientId).ToListAsync();
    }

    public async Task<Calendar?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Calendar entity)
    {
        var filter = Builders<Calendar>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity);
    }

}
