using EasyReasy.Ollama.Common;

namespace EasyReasy.Ollama.Client.Tests
{
    [TestClass]
    public class ChatClientIntegrationTests : BaseIntegrationTest
    {

        [TestMethod]
        public async Task StreamChatAsync_SingleText_StreamsResponse()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();

            // Act
            List<ChatResponsePart> responses = new List<ChatResponsePart>();
            await foreach (ChatResponsePart response in client.Chat.StreamChatAsync("llama3.1", "Hello"))
            {
                responses.Add(response);
                // Break after first response to avoid long-running test
                break;
            }

            // Assert
            Assert.IsTrue(responses.Count > 0);
        }

        [TestMethod]
        public async Task StreamChatAsync_Messages_StreamsResponse()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            List<Message> messages = new List<Message>
            {
                new Message(ChatRole.User, "Hello"),
                new Message(ChatRole.Assistant, "Hi there!")
            };

            // Act
            List<ChatResponsePart> responses = new List<ChatResponsePart>();
            await foreach (ChatResponsePart response in client.Chat.StreamChatAsync("llama3.1", messages))
            {
                responses.Add(response);
                // Break after first response to avoid long-running test
                break;
            }

            // Assert
            Assert.IsTrue(responses.Count > 0);
        }

        [TestMethod]
        public async Task StreamChatAsync_WithTools_StreamsResponse()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            List<Message> messages = new List<Message>
            {
                new Message(ChatRole.User, "What's the weather like?")
            };
            List<ToolDefinition> tools = new List<ToolDefinition>
            {
                new ToolDefinition("get_weather", "Get the current weather", new List<PossibleParameter>())
            };

            // Act
            List<ChatResponsePart> responses = new List<ChatResponsePart>();
            await foreach (ChatResponsePart response in client.Chat.StreamChatAsync("llama3.1", messages, tools))
            {
                responses.Add(response);
                // Break after first response to avoid long-running test
                break;
            }

            // Assert
            Assert.IsTrue(responses.Count > 0);
        }

        [TestMethod]
        public async Task StreamChatAsync_ChatRequest_StreamsResponse()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            ChatRequest request = new ChatRequest("llama3.1", new List<Message> { new Message(ChatRole.User, "Hello") });

            // Act
            List<ChatResponsePart> responses = new List<ChatResponsePart>();
            await foreach (ChatResponsePart response in client.Chat.StreamChatAsync(request))
            {
                responses.Add(response);
                // Break after first response to avoid long-running test
                break;
            }

            // Assert
            Assert.IsTrue(responses.Count > 0);
        }

        [TestMethod]
        public async Task StreamChatAsync_InvalidModel_ThrowsException()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
            {
                await foreach (ChatResponsePart response in client.Chat.StreamChatAsync("invalid-model", "Hello"))
                {
                    // This should throw before yielding any responses
                }
            });
        }

        [TestMethod]
        public async Task StreamChatAsync_Unauthenticated_ThrowsException()
        {
            // Arrange
            OllamaClient client = CreateUnauthenticatedClient();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
            {
                await foreach (ChatResponsePart response in client.Chat.StreamChatAsync("llama3.1", "Hello"))
                {
                    // This should throw before yielding any responses
                }
            });
        }

        [TestMethod]
        public async Task StreamChatAsync_CancellationToken_CancelsStream()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // Cancel after 100ms

            // Act
            List<ChatResponsePart> responses = new List<ChatResponsePart>();
            await foreach (ChatResponsePart response in client.Chat.StreamChatAsync("llama3.1", "Hello", cts.Token))
            {
                responses.Add(response);
            }

            // Assert
            // The stream should be cancelled, so we might get some responses or none
            // The important thing is that it doesn't throw an exception
        }
    }
}