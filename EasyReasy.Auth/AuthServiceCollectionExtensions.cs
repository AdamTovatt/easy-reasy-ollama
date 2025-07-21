using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EasyReasy.Auth
{
    /// <summary>
    /// Extension methods for configuring authentication and authorization for EasyReasy.
    /// </summary>
    public static class AuthServiceCollectionExtensions
    {
        /// <summary>
        /// Adds JWT authentication and authorization policies for EasyReasy.
        /// </summary>
        /// <param name="services">The service collection to add authentication to.</param>
        /// <param name="jwtSecret">The secret key used to sign JWT tokens.</param>
        /// <param name="issuer">The expected issuer for JWT tokens. If null, issuer validation is disabled.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddEasyReasyAuth(
            this IServiceCollection services,
            string jwtSecret,
            string? issuer = null)
        {
            byte[] key = Encoding.UTF8.GetBytes(jwtSecret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = issuer != null,
                    ValidIssuer = issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiKeyOnly", policy =>
                    policy.RequireClaim("auth_type", "apikey"));
                options.AddPolicy("UserOnly", policy =>
                    policy.RequireClaim("auth_type", "user"));
            });

            return services;
        }
    }
}