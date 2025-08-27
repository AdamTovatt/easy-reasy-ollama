using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Common;

namespace EasyReasy.Ollama.Client.Tests
{
    [TestClass]
    public class RemoteIntegrationTests
    {
        private static IOllamaClient _client = null!;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            try
            {
                // Load environment variables from file
                EnvironmentVariableHelper.LoadVariablesFromFile("..\\..\\env.txt");
                EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(RemoteIntegrationEnvironmentVariables));

                string remoteUrl = RemoteIntegrationEnvironmentVariables.RemoteUrl.GetValue();
                string apiKey = RemoteIntegrationEnvironmentVariables.ApiKey.GetValue();

                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(remoteUrl);

                _client = await OllamaClient.CreateAuthorizedAsync(httpClient, apiKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during remote integration test setup: {ex.Message}");
                throw;
            }
        }

        [TestMethod]
        public async Task GetEmbeddingWorks()
        {
            const string text = "Hello world";
            EmbeddingResponse response = await _client.Embeddings.GetEmbeddingsAsync("nomic-embed-text", text);

            Console.WriteLine($"Embedding for '{text}': {string.Join(", ", response.Embeddings)}");
        }

        [TestMethod]
        public async Task StreamChatAsync_SingleText_StreamsResponse()
        {
            // Arrange
            const string prompt = "Hello";

            // Debug: Let's first test if the server is reachable
            try
            {
                using HttpClient debugClient = new HttpClient();
                debugClient.BaseAddress = new Uri(RemoteIntegrationEnvironmentVariables.RemoteUrl.GetValue());

                // Test a simple GET request to see if the server is reachable
                HttpResponseMessage debugResponse = await debugClient.GetAsync("/");
                Console.WriteLine($"Server reachable: {debugResponse.StatusCode}");

                // Test if the chat endpoint exists
                HttpResponseMessage chatResponse = await debugClient.GetAsync("/api/chat/stream");
                Console.WriteLine($"Chat endpoint status: {chatResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug request failed: {ex.Message}");
            }

            // Act
            List<ChatResponsePart> responses = new List<ChatResponsePart>();
            await foreach (ChatResponsePart response in _client.Chat.StreamChatAsync("llama3.1", prompt))
            {
                responses.Add(response);
                // Break after first response to avoid long-running test
                break;
            }

            // Assert
            Assert.IsTrue(responses.Count > 0);
            Console.WriteLine($"Received {responses.Count} response parts from remote chat");
        }
    }
}
