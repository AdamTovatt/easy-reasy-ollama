using EasyReasy.Auth;
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
            EnvironmentVariables.EnvironmentVariables.ValidateVariableNamesIn(typeof(EnvironmentVariable));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register ResourceManager as a singleton
            ResourceManager resourceManager = await ResourceManager.CreateInstanceAsync();
            builder.Services.AddSingleton(resourceManager);

            // Register IOllamaChatService and OllamaChatService
            string ollamaUrl = EnvironmentVariables.EnvironmentVariables.GetVariable(EnvironmentVariable.OllamaUrl);
            string ollamaModel = EnvironmentVariables.EnvironmentVariables.GetVariable(EnvironmentVariable.OllamaModelName);

            builder.Services.AddSingleton<IOllamaChatService>(provider =>
            {
                ILogger<IOllamaService> logger = provider.GetRequiredService<ILogger<IOllamaService>>();
                return OllamaChatService.Create(ollamaUrl, ollamaModel, logger);
            });

            builder.Services.AddSingleton<IOllamaEmbeddingService>(provider =>
            {
                ILogger<IOllamaService> logger = provider.GetRequiredService<ILogger<IOllamaService>>();
                return OllamaEmbeddingService.Create(ollamaUrl, ollamaModel, logger);
            });

            // Register TenantService
            builder.Services.AddSingleton<ITenantService, EnvironmentVariablesTenantService>();

            // Add JWT authentication and authorization
            string jwtSecret = EnvironmentVariables.EnvironmentVariables.GetVariable(EnvironmentVariable.JwtSigningSecret);
            builder.Services.AddEasyReasyAuth(jwtSecret, issuer: "OllamaServer");

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEasyReasyAuth();

            app.MapControllers();

            app.Run();
        }
    }
}
