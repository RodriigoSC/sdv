using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace SDV.Infra.Data.MongoDB;

public static class MongoDbConfig
{
    private static bool _configured = false;

        public static void Configure()
        {
            if (_configured) return;

            // Configura GUIDs para representação padrão (string segura)
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            _configured = true;
        }

}
