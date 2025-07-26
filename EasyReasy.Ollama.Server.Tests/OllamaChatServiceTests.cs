using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Server.Providers;
using EasyReasy.Ollama.Server.Services.Ollama;
using EasyReasy.Ollama.Server.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace EasyReasy.Ollama.Server.Tests
{
    [TestClass]
    public class OllamaChatServiceTests
    {
        private const string _defaultChatModel = "gemma3";

        private static string _url = null!;
        private static IAllowedModelsProvider _allowedModelsProvider = null!;
        private static ILogger<OllamaChatService> _logger = null!;
        private static OllamaChatService _chatService = null!;

        [ClassInitialize]
        public static void BeforeAll(TestContext testContext)
        {
            _url = OllamaIntegrationEnvironmentVariables.OllamaUrl.GetValue();
            _allowedModelsProvider = new EnvironmentVariablesAllowedModelsProvider();
            _logger = new ConsoleLogger<OllamaChatService>();

            _chatService = OllamaChatService.Create(_url, _defaultChatModel, _logger, _allowedModelsProvider);
        }

        [TestMethod]
        public async Task ChatService_ReturnsResponse()
        {
            StringBuilder totalResponse = new StringBuilder();

            IAsyncEnumerable<string> response = _chatService.GetResponseAsync("Hello, who are you?", CancellationToken.None);

            await foreach (string message in response)
            {
                totalResponse.Append(message);
            }

            Console.WriteLine(totalResponse.ToString());
        }
    }
}