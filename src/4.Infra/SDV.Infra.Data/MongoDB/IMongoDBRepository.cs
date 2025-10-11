using MongoDB.Driver;

namespace SDV.Infra.Data.MongoDB;

public interface IMongoDBRepository
{
    IMongoDBRepository SetDatabase(string databaseName);

    bool ExistCollection(string collectionName);

    void CreateCollection(string collectionName);

    void CreateIndexDatabase(string collectionName, List<string> indexes);

    IMongoCollection<T> GetCollection<T>(string collectionName);

}
