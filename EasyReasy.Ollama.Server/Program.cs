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

            // Register IOllamaService and OllamaService
            string ollamaUrl = EnvironmentVariables.EnvironmentVariables.GetVariable(EnvironmentVariable.OllamaUrl);
            string ollamaModel = EnvironmentVariables.EnvironmentVariables.GetVariable(EnvironmentVariable.OllamaModelName);
            IOllamaService ollamaService = OllamaService.Create(ollamaUrl, ollamaModel);
            builder.Services.AddSingleton(ollamaService);

            // Register TenantService
            builder.Services.AddSingleton<ITenantService, TenantService>();

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
