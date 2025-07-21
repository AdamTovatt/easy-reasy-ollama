using Microsoft.AspNetCore.Builder;

namespace EasyReasy.Auth
{
    /// <summary>
    /// Extension methods for configuring the EasyReasy authentication middleware pipeline.
    /// </summary>
    public static class AuthApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds authentication, authorization, claims injection, and (optionally) progressive delay middleware to the application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="enableProgressiveDelay">If true, enables progressive delay for repeated unauthorized requests. Default is true.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseEasyReasyAuth(this IApplicationBuilder app, bool enableProgressiveDelay = true)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<ClaimsInjectionMiddleware>();
            if (enableProgressiveDelay)
                app.UseMiddleware<ProgressiveDelayMiddleware>();
            return app;
        }
    }
}