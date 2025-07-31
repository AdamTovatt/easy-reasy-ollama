using EasyReasy.Auth;
using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Server.Models.Tenants;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace EasyReasy.Ollama.Server.Services.Tenants
{
    /// <summary>
    /// Tenant service that loads tenant info from environment variables at startup.
    /// </summary>
    public class EnvironmentVariablesTenantService : ITenantService, IAuthRequestValidationService
    {
        private readonly ConcurrentDictionary<string, TenantInfo> _apiKeyToTenant = new ConcurrentDictionary<string, TenantInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariablesTenantService"/> class.
        /// </summary>
        public EnvironmentVariablesTenantService()
        {
            IEnumerable<string> tenantInfoValues = EnvironmentVariables.TenantInfo.GetAllValues();

            foreach (string value in tenantInfoValues)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

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

        /// <inheritdoc />
        public async Task<AuthResponse?> ValidateApiKeyRequestAsync(ApiKeyAuthRequest request, IJwtTokenService jwtTokenService)
        {
            await Task.CompletedTask;

            TenantInfo? tenant = GetTenantInfoByApiKey(request.ApiKey);
            if (tenant == null) return null;

            DateTime expiresAt = DateTime.UtcNow.AddHours(1);
            string token = jwtTokenService.CreateToken(
                subject: tenant.TenantId,
                authType: "apikey",
                additionalClaims: new[] { new Claim("tenant_id", tenant.TenantId) },
                roles: Array.Empty<string>(),
                expiresAt: expiresAt);

            return new AuthResponse(token, expiresAt.ToString("o"));
        }

        /// <inheritdoc />
        public async Task<AuthResponse?> ValidateLoginRequestAsync(LoginAuthRequest request, IJwtTokenService jwtTokenService)
        {
            await Task.CompletedTask;

            // Login authentication not implemented in this project
            throw new NotImplementedException("Login authentication not implemented in this project");
        }
    }
}