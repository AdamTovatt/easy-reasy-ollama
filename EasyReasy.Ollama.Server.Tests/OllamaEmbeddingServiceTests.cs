using EasyReasy.Ollama.Server.Services.Ollama;
using EasyReasy.Ollama.Server.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using EasyReasy.Ollama.Server.Providers;
using EasyReasy.EnvironmentVariables;

namespace EasyReasy.Ollama.Server.Tests
{
    [TestClass]
    public class OllamaEmbeddingServiceTests
    {
        private const string _defaultEmbeddingModel = "nomic-embed-text";

        private static string _url = null!;
        private static string _model = null!;
        private static ILogger<OllamaEmbeddingService> _logger = null!;
        private static IAllowedModelsProvider _allowedModelsProvider = null!;

        [ClassInitialize]
        public static void BeforeAll(TestContext testContext)
        {
            _url = OllamaIntegrationEnvironmentVariables.OllamaUrl.GetValue();
            _model = _defaultEmbeddingModel;
            _logger = new ConsoleLogger<OllamaEmbeddingService>();
            _allowedModelsProvider = new EnvironmentVariablesAllowedModelsProvider();
        }

        [TestMethod]
        public async Task EmbeddingService_ReturnsEmbeddings()
        {
            using OllamaEmbeddingService service = OllamaEmbeddingService.Create(_url, _model, _logger, _allowedModelsProvider);

            float[] embedding = await service.GetEmbeddingsAsync("Hello, world!", CancellationToken.None);
            Assert.IsNotNull(embedding);
            Assert.IsTrue(embedding.Length > 0);

            Console.WriteLine($"[{string.Join(',', embedding)}]");
        }
    }
}