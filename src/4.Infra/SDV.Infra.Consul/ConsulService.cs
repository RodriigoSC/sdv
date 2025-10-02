using Consul;

namespace SDV.Infra.Consul;

public class ConsulService : IConsulService
{
    private readonly ConsulClient _client;

    public ConsulService(string consulUrl)
    {
        _client = new ConsulClient(config =>
        {
            config.Address = new Uri(consulUrl);
        });
    }

    public async Task<string?> GetValueAsync(string path, string key)
    {
        var fullKey = $"{path}/{key}";
        var kvPair = await _client.KV.Get(fullKey);

        if (kvPair.Response == null)
            return null;

        return System.Text.Encoding.UTF8.GetString(kvPair.Response.Value);
    }
}
