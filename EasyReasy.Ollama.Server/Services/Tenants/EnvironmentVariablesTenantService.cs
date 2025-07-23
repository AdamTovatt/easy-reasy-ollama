using EasyReasy.Ollama.Server.Models.Tenants;
using System.Collections.Concurrent;

namespace EasyReasy.Ollama.Server.Services.Tenants
{
    /// <summary>
    /// Tenant service that loads tenant info from environment variables at startup.
    /// </summary>
    public class EnvironmentVariablesTenantService : ITenantService
    {
        private readonly ConcurrentDictionary<string, TenantInfo> _apiKeyToTenant = new ConcurrentDictionary<string, TenantInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariablesTenantService"/> class.
        /// </summary>
        public EnvironmentVariablesTenantService()
        {
            int index = 1;

            while (true)
            {
                string environmentVariableName = index == 1 ? "TENANT_INFO" : $"TENANT_INFO{index}";
                string? value = Environment.GetEnvironmentVariable(environmentVariableName);

                if (string.IsNullOrWhiteSpace(value))
                    break;

                string[] parts = value.Split(',', 2);

                if (parts.Length == 2)
                {
                    string tenantId = parts[0].Trim();
                    string apiKey = parts[1].TrimStart();
                    if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(apiKey))
                    {
                        TenantInfo info = new TenantInfo(tenantId, apiKey);
                        _apiKeyToTenant[apiKey] = info;
                    }
                }

                index++;
            }
        }

        /// <inheritdoc />
        public TenantInfo? GetTenantInfoByApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                return null;

            _apiKeyToTenant.TryGetValue(apiKey, out TenantInfo? info);

            return info;
        }
    }
}