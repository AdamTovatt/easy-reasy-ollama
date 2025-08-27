using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Common;
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
            StringBuilder totalResponseTextContent = new StringBuilder();

            // Test scenario where we provide tool definitions to the model
            // Note: This test demonstrates what SHOULD work if the service supported tool definitions
            List<Message> messages = new List<Message>
            {
                new Message(ChatRole.System, "You are a helpful assistant with access to a weather tool function. Call the tool with the correct parameters if you need it."),
                new Message(ChatRole.User, "What's the weather like in London?")
            };

            PossibleParameter possibleParameter = new PossibleParameter("city", "string", "Name of the city", true);
            ToolDefinition toolDefinition = new ToolDefinition("get_current_weather", "Get the current weather for a city", new List<PossibleParameter>() { possibleParameter });

            IAsyncEnumerable<ChatResponsePart> response = _chatService.GetResponseAsync(messages, new List<ToolDefinition>() { toolDefinition }, CancellationToken.None);

            ToolCall? toolCall = null;
            int responseParts = 0;

            await foreach (ChatResponsePart part in response)
            {
                responseParts++;

                if (part.ToolCall != null)
                {
                    Console.WriteLine($"This was a tool call");

                    Console.WriteLine($"Tool call json: {part.ToolCall.ToJson()}");
                    toolCall = part.ToolCall;
                }
                else
                {
                    totalResponseTextContent.Append(part.Message);
                }
            }

            Assert.AreEqual(1, responseParts, "Response should only have one part which is the tool call.");
            Assert.AreEqual(string.Empty, totalResponseTextContent.ToString(), "Total text content result should be empty.");
            Assert.IsNotNull(toolCall);
            Assert.IsNotNull(toolCall.Function);
            Assert.AreEqual(toolDefinition.Name, toolCall.Function.Name);
            Assert.IsNotNull(toolCall.Function.Arguments);
            Assert.AreEqual(1, toolCall.Function.Arguments.Count());
        }

        [TestMethod]
        public async Task ChatService_ModelUnderstandToolCallResults_WithToolDefinitions()
        {
            // Test scenario where we provide tool definitions to the model
            // Note: This test demonstrates what SHOULD work if the service supported tool definitions
            List<Message> messages = new List<Message>
            {
                new Message(ChatRole.System, "You are a helpful assistant with access to a weather tool function. Call the tool with the correct parameters if you need it."),
                new Message(ChatRole.User, "What's the weather like in London? Please give me the temperature in both farenheight and celsius")
            };

            PossibleParameter possibleParameter = new PossibleParameter("city", "string", "Name of the city", true);
            ToolDefinition toolDefinition = new ToolDefinition("GetCurrentWeather", "Get the current weather for a city", new List<PossibleParameter>() { possibleParameter });

            IAsyncEnumerable<ChatResponsePart> response = _chatService.GetResponseAsync(messages, new List<ToolDefinition>() { toolDefinition }, CancellationToken.None);

            ToolCall? toolCall = null;
            StringBuilder totalResponseTextContent = new StringBuilder();

            await foreach (ChatResponsePart part in response)
            {
                if (part.ToolCall != null)
                {
                    toolCall = part.ToolCall;
                }
                else
                {
                    totalResponseTextContent.Append(part.Message);
                }
            }

            Assert.IsNotNull(toolCall, "Tool call should not be null. Response was: " + totalResponseTextContent.ToString());
            Assert.IsNotNull(toolCall.Function, "Tool call function should not be null");
            Assert.IsNotNull(toolCall.Function.Name);

            Message toolCallMessage = new Message(toolCall.Function.Name, "{\"cityName\":\"London\",\"temperatureCelsius\":\"22\",\"skyCondition\":\"clear\",\"chanceOfRain\":\"0\"}");

            messages.Add(toolCallMessage);

            response = _chatService.GetResponseAsync(messages, new List<ToolDefinition>() { toolDefinition }, CancellationToken.None);
            toolCall = null;
            totalResponseTextContent.Clear();

            await foreach (ChatResponsePart part in response)
            {
                if (part.ToolCall != null)
                {
                    toolCall = part.ToolCall;
                }
                else
                {
                    totalResponseTextContent.Append(part.Message);
                }
            }

            Assert.IsNull(toolCall, "Tool call should be null when getting text response to previous tool cal result.");
            Assert.IsFalse(string.IsNullOrEmpty(totalResponseTextContent.ToString()), "Total text response should not be null or empty");

            Console.WriteLine(totalResponseTextContent.ToString());
        }
    }
}