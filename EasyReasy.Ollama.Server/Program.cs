using EasyReasy.Auth;
using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Server.Providers;
using EasyReasy.Ollama.Server.Services.Ollama;
using EasyReasy.Ollama.Server.Services.Tenants;

namespace EasyReasy.Ollama.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Validate environment variables at startup
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(EnvironmentVariables));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register ResourceManager as a singleton
            ResourceManager resourceManager = await ResourceManager.CreateInstanceAsync();
            builder.Services.AddSingleton(resourceManager);

            // Register IAllowedModelsProvider
            builder.Services.AddSingleton<IAllowedModelsProvider, EnvironmentVariablesAllowedModelsProvider>();

            // Register IOllamaServiceFactory with concrete loggers
            string ollamaUrl = EnvironmentVariables.OllamaUrl.GetValue();
            builder.Services.AddSingleton<IOllamaServiceFactory>(provider =>
            {
                ILogger<OllamaChatService> chatLogger = provider.GetRequiredService<ILogger<OllamaChatService>>();
                ILogger<OllamaEmbeddingService> embeddingLogger = provider.GetRequiredService<ILogger<OllamaEmbeddingService>>();
                IAllowedModelsProvider allowedModelsProvider = provider.GetRequiredService<IAllowedModelsProvider>();
                return new OllamaServiceFactory(ollamaUrl, chatLogger, embeddingLogger, allowedModelsProvider);
            });

            // Register default IOllamaChatService using the first allowed model
            builder.Services.AddSingleton<IOllamaChatService>(provider =>
            {
                IOllamaServiceFactory factory = provider.GetRequiredService<IOllamaServiceFactory>();
                IAllowedModelsProvider allowedModelsProvider = provider.GetRequiredService<IAllowedModelsProvider>();
                string defaultModel = allowedModelsProvider.GetAllowedModels().First();
                return factory.GetChatService(defaultModel);
            });

            // Register TenantService
            builder.Services.AddSingleton<ITenantService, EnvironmentVariablesTenantService>();
            
            // Register AuthValidationService (using the same instance as TenantService)
            builder.Services.AddSingleton<IAuthRequestValidationService>(provider =>
                (IAuthRequestValidationService)provider.GetRequiredService<ITenantService>());

            // Add JWT authentication and authorization
            string jwtSecret = EnvironmentVariables.JwtSigningSecret.GetValue();
            builder.Services.AddEasyReasyAuth(jwtSecret, issuer: "OllamaServer");

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // UseEasyReasyAuth includes UseAuthentication() and UseAuthorization()
            app.UseEasyReasyAuth();

            // Add auth endpoints
            app.AddAuthEndpoints(
                app.Services.GetRequiredService<IAuthRequestValidationService>(), 
                allowApiKeys: true, 
                allowUsernamePassword: false);

            app.MapControllers();

            app.Run();
        }
    }
}
