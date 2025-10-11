using SDV.Infra.Data.MongoDB;

namespace SDV.Infra.Data.Repository.Common;

public abstract class CommonRepository
{
    protected readonly IMongoDBRepository _mongoDbRepository;

    protected CommonRepository(IMongoDBRepository mongoDbRepository)
    {
        _mongoDbRepository = mongoDbRepository;
    }

    protected void MongoDbVerifyAndCreateIndex(string collection, List<string> index)
    {
        try
        {
            var collectionExist = _mongoDbRepository.ExistCollection(collection);

            if (!collectionExist)
            {
                _mongoDbRepository.CreateCollection(collection);

                if (index != null && index.Count > 0)
                {
                    _mongoDbRepository.CreateIndexDatabase(collection, index);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar índice no MongoDB: {ex.Message}");
        }
    }
}
