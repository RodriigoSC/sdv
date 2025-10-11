using System;
using VaultSharp;
using VaultSharp.V1.AuthMethods.UserPass;
using VaultSharp.V1.Commons;

namespace SDV.Infra.Vault;

public class VaultService : IVaultService
{
    private readonly IVaultClient _client;

    public VaultService(string vaultAddress, string userName, string password)
    {
        var authMethod = new UserPassAuthMethodInfo(userName, password);
        var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);
        _client = new VaultClient(vaultClientSettings);
    }

    public async Task<string?> GetKeyAsync(string path, string key, string mountPoint)
    {
        Secret<SecretData> secret = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            path,
            mountPoint: mountPoint
        );

        if (secret?.Data?.Data.ContainsKey(key) ?? false)
            return secret.Data.Data[key]?.ToString();

        return null;
    }

}
