using System.Text.Json.Serialization;

namespace EasyReasy.Ollama.Server.Models.Tenants
{
    /// <summary>
    /// Model representing tenant information.
    /// </summary>
    public class TenantInfo
    {
        [JsonPropertyName("tenantId")]
        public string TenantId { get; }

        [JsonPropertyName("apiKey")]
        public string ApiKey { get; }

        public TenantInfo(string tenantId, string apiKey)
        {
            TenantId = tenantId;
            ApiKey = apiKey;
        }
    }
}