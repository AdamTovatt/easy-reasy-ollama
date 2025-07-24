using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Server.Services.Ollama;
using EasyReasy.Ollama.Server.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace EasyReasy.Ollama.Server.Tests
{
    [TestClass]
    public class OllamaChatServiceTests
    {
        private static string _url = null!;
        private static string _model = null!;
        private static ILogger<IOllamaService> _logger = null!;

        [ClassInitialize]
        public static void BeforeAll(TestContext testContext)
        {
            EnvironmentVariableHelper.LoadVariablesFromFile("env.txt");
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(OllamaIntegrationEnvironmentVariables));
            _url = OllamaIntegrationEnvironmentVariables.OllamaUrl.GetValue();
            _model = OllamaIntegrationEnvironmentVariables.OllamaModelName.GetValue();
            _logger = new ConsoleLogger<IOllamaService>();
        }

        [TestMethod]
        public async Task ChatService_ReturnsResponse()
        {
            using OllamaChatService service = OllamaChatService.Create(_url, _model, _logger);

            StringBuilder totalResponse = new StringBuilder();

            IAsyncEnumerable<string> response = service.GetResponseAsync("Hello, who are you?", CancellationToken.None);

            await foreach (string message in response)
            {
                totalResponse.Append(message);
            }

            Console.WriteLine(totalResponse.ToString());
        }
    }
}