using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Server.Providers;
using EasyReasy.Ollama.Server.Services.Ollama;
using EasyReasy.Ollama.Server.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using System.Text;
using EasyReasy.Ollama.Common;

namespace EasyReasy.Ollama.Server.Tests
{
    [TestClass]
    public class OllamaChatServiceTests
    {
        private const string _defaultChatModel = "llama3.1";

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

            IAsyncEnumerable<ChatResponsePart> response = _chatService.GetResponseAsync("Hello, who are you?", CancellationToken.None);

            await foreach (ChatResponsePart part in response)
            {
                totalResponse.Append(part.Message);
            }

            Console.WriteLine(totalResponse.ToString());
        }

        [TestMethod]
        public async Task ChatService_ReturnsResponse_WithMessageList()
        {
            StringBuilder totalResponse = new StringBuilder();

            List<Message> messages = new List<Message>
            {
                new Message(ChatRole.System, "You are a helpful assistant."),
                new Message(ChatRole.User, "Hello, who are you?")
            };

            IAsyncEnumerable<ChatResponsePart> response = _chatService.GetResponseAsync(messages, CancellationToken.None);

            await foreach (ChatResponsePart part in response)
            {
                totalResponse.Append(part.Message);
            }

            Console.WriteLine(totalResponse.ToString());
        }

        [TestMethod]
        public async Task ChatService_ModelGeneratesToolCalls_WithToolDefinitions()
        {
            StringBuilder totalResponse = new StringBuilder();

            // Test scenario where we provide tool definitions to the model
            // Note: This test demonstrates what SHOULD work if the service supported tool definitions
            List<Message> messages = new List<Message>
            {
                new Message(ChatRole.System, "You are a helpful assistant with access to a weather tool function. Call the tool with the correct parameters if you need it."),
                new Message(ChatRole.User, "What's the weather like in London?")
            };

            PossibleParameter possibleParameter = new PossibleParameter("city", "string", "Name of the city", true);
            ToolDefinition toolDefinition = new ToolDefinition("Get current weather", "Get the current weather for a city", new List<PossibleParameter>() { possibleParameter });

            IAsyncEnumerable<ChatResponsePart> response = _chatService.GetResponseAsync(messages, new List<ToolDefinition>() { toolDefinition }, CancellationToken.None);

            List<ToolCall> toolCalls = new List<ToolCall>();

            await foreach (ChatResponsePart part in response)
            {
                if (part.ToolCalls != null)
                {
                    Console.WriteLine($"This was a tool call");

                    foreach (ToolCall call in part.ToolCalls)
                    {
                        toolCalls.Add(call);
                    }
                }
                else
                {
                    totalResponse.Append(part.Message);
                }
            }

            Console.WriteLine("Model response (text only):");
            Console.WriteLine(totalResponse.ToString());
            Console.WriteLine();
            Console.WriteLine("Note: This test demonstrates the limitation - the model doesn't know about available tools");
            Console.WriteLine("because the service doesn't provide tool definitions to the model.");
            Console.WriteLine("The model would need to be told what tools are available via the ChatRequest.");
        }
    }
}