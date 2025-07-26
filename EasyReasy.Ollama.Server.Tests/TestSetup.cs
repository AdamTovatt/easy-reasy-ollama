using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Server.Providers;
using EasyReasy.Ollama.Server.Services.Ollama;
using EasyReasy.Ollama.Server.Tests.TestUtilities;
using Microsoft.Extensions.Logging;

namespace EasyReasy.Ollama.Server.Tests
{
    /// <summary>
    /// Global test setup that runs once before all tests to ensure all required models are available.
    /// </summary>
    [TestClass]
    public class TestSetup
    {
        private static bool _isInitialized = false;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Ensures all allowed models are downloaded before any tests run.
        /// This method is called by MSTest's assembly initialization.
        /// </summary>
        [AssemblyInitialize]
        public static async Task InitializeModels(TestContext testContext)
        {
            lock (_lockObject)
            {
                if (_isInitialized)
                    return;
                _isInitialized = true;
            }

            try
            {
                // Load environment variables
                EnvironmentVariableHelper.LoadVariablesFromFile("env.txt");
                EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(OllamaIntegrationEnvironmentVariables));

                string ollamaUrl = OllamaIntegrationEnvironmentVariables.OllamaUrl.GetValue();
                IAllowedModelsProvider allowedModelsProvider = new EnvironmentVariablesAllowedModelsProvider();
                ILogger<OllamaChatService> chatLogger = new ConsoleLogger<OllamaChatService>();
                ILogger<OllamaEmbeddingService> embeddingLogger = new ConsoleLogger<OllamaEmbeddingService>();

                // Create factory and download all allowed models
                using OllamaServiceFactory factory = new OllamaServiceFactory(ollamaUrl, chatLogger, embeddingLogger, allowedModelsProvider);

                Console.WriteLine("Setting up test environment - downloading required models...");
                await factory.PullModelsAsync();
                Console.WriteLine("Test environment setup complete - all required models are available.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during test setup: {ex.Message}");
                throw;
            }
        }
    }
}