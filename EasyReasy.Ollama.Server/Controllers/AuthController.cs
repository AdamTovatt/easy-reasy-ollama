using EasyReasy.Auth;
using EasyReasy.Ollama.Common;
using EasyReasy.Ollama.Server.Models.Tenants;
using EasyReasy.Ollama.Server.Services.Tenants;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EasyReasy.Ollama.Server.Controllers
{
    /// <summary>
    /// Authentication controller for issuing JWT tokens.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ITenantService _tenantService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="jwtTokenService">The JWT token service.</param>
        /// <param name="tenantService">The tenant service.</param>
        public AuthController(IJwtTokenService jwtTokenService, ITenantService tenantService)
        {
            _jwtTokenService = jwtTokenService;
            _tenantService = tenantService;
        }

        /// <summary>
        /// Authenticates using an API key and returns a JWT if valid.
        /// </summary>
        /// <param name="request">The API key authentication request.</param>
        /// <returns>A JWT token if authentication is successful.</returns>
        [HttpPost("apikey")]
        public IActionResult AuthenticateWithApiKey([FromBody] ApiKeyAuthRequest request)
        {
            TenantInfo? tenant = _tenantService.GetTenantInfoByApiKey(request.ApiKey);
            if (tenant != null)
            {
                string token = _jwtTokenService.CreateToken(
                    subject: tenant.TenantId,
                    authType: "apikey",
                    additionalClaims: new[] { new Claim("tenant_id", tenant.TenantId) },
                    roles: Array.Empty<string>(),
                    expiresAt: DateTime.UtcNow.AddHours(1));

                return Ok(new { token });
            }
            return Unauthorized();
        }
    }
}