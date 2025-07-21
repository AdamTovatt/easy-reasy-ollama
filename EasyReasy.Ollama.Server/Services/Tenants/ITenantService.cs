using EasyReasy.Ollama.Server.Models.Tenants;

namespace EasyReasy.Ollama.Server.Services.Tenants
{
    /// <summary>
    /// Service interface for tenant-related operations.
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Gets tenant information by API key.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <returns>The tenant info, or null if not found.</returns>
        TenantInfo? GetTenantInfoByApiKey(string apiKey);
    }
}