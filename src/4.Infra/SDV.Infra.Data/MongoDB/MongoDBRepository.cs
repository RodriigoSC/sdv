using MongoDB.Bson;
using MongoDB.Driver;

namespace SDV.Infra.Data.MongoDB
{
    public class MongoDBRepository : IMongoDBRepository
    {
        private IMongoDatabase? _database;
        private readonly IMongoClient _client;

        public MongoDBRepository(string connectionString)
        {
            _client = new MongoClient(connectionString);
        }

        public IMongoDBRepository SetDatabase(string databaseName)
        {
            _database = _client.GetDatabase(databaseName);
            return this;
        }

        public bool ExistCollection(string collectionName)
        {
            if (_database == null) throw new InvalidOperationException("Database not selected.");
            var filter = new BsonDocument("name", collectionName);
            var collections = _database.ListCollections(new ListCollectionsOptions { Filter = filter });
            return collections.Any();
        }

        public void CreateCollection(string collectionName)
        {
            if (_database == null) throw new InvalidOperationException("Database not selected.");
            _database.CreateCollection(collectionName);
        }

        public void CreateIndexDatabase(string collectionName, List<string> indexes)
        {
            if (_database == null) throw new InvalidOperationException("Database not selected.");

            var collection = _database.GetCollection<BsonDocument>(collectionName);

            foreach (var index in indexes)
            {
                var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending(index);
                var indexModel = new CreateIndexModel<BsonDocument>(indexKeys);
                collection.Indexes.CreateOne(indexModel);
            }
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            if (_database == null) throw new InvalidOperationException("Database not selected.");
            return _database.GetCollection<T>(collectionName);
        }
    }
}
