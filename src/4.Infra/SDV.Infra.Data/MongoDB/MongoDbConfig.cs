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

            try
            {
                // Configura GUIDs para representação padrão (string segura)
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            }
            catch (BsonSerializationException)
            {
                // Ignora o erro se o serializador já estiver registrado.
                // Isso é comum em ambientes de teste onde a configuração pode ser executada várias vezes.
            }

            _configured = true;
        }

}
