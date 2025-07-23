using EasyReasy.Ollama.Server.Services.Ollama;
using EasyReasy.Ollama.Server.Tests.TestUtilities;
using Microsoft.Extensions.Logging;

namespace EasyReasy.Ollama.Server.Tests
{
    [TestClass]
    public class OllamaEmbeddingServiceTests
    {
        private static string _url = null!;
        private static string _model = null!;
        private static ILogger<IOllamaService> _logger = null!;

        [ClassInitialize]
        public static void BeforeAll(TestContext testContext)
        {
            EnvironmentVariables.EnvironmentVariables.LoadFromFile("env.txt");
            EnvironmentVariables.EnvironmentVariables.ValidateVariableNamesIn(typeof(OllamaIntegrationEnvironmentVariables));

            _url = EnvironmentVariables.EnvironmentVariables.GetVariable(OllamaIntegrationEnvironmentVariables.OllamaUrl);
            _model = EnvironmentVariables.EnvironmentVariables.GetVariable(OllamaIntegrationEnvironmentVariables.OllamaModelName);
            _logger = new ConsoleLogger<IOllamaService>();
        }

        [TestMethod]
        public async Task EmbeddingService_ReturnsEmbeddings()
        {
            using OllamaEmbeddingService service = OllamaEmbeddingService.Create(_url, _model, _logger);

            if (!await service.IsModelAvailableAsync(_model))
            {
                await service.PullModelAsync(_model);
            }

            float[] embedding = await service.GetEmbeddingsAsync("Hello, world!", CancellationToken.None);
            Assert.IsNotNull(embedding);
            Assert.IsTrue(embedding.Length > 0);

            Console.WriteLine($"[{string.Join(',', embedding)}]");
        }
    }
}