using System;

namespace SDV.Infra.Consul;

public interface IConsulService
{
    Task<string?> GetValueAsync(string path, string key);
}
