using System;

namespace SDV.Infra.Vault;

public interface IVaultService
{
    Task<string?> GetKeyAsync(string path, string key, string mountPoint);
}
