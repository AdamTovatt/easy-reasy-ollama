using EasyReasy.Ollama.Common;
using System.Runtime.CompilerServices;

namespace EasyReasy.Ollama.Client
{
    /// <summary>
    /// Implementation of the chat client that handles all chat-related operations.
    /// </summary>
    public class ChatClient : IChatClient
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaClient _mainClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use.</param>
        /// <param name="mainClient">The main Ollama client for authorization.</param>
        public ChatClient(HttpClient httpClient, OllamaClient mainClient)
        {
            _httpClient = httpClient;
            _mainClient = mainClient;
        }

        /// <summary>
        /// Streams chat responses for a single text input.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="text">The user input text.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        public IAsyncEnumerable<ChatResponsePart> StreamChatAsync(string modelName, string text, CancellationToken cancellationToken = default)
        {
            List<Message> messages = new List<Message>
            {
                new Message(ChatRole.User, text)
            };

            return StreamChatAsync(modelName, messages, cancellationToken);
        }

        /// <summary>
        /// Streams chat responses for a list of messages.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        public IAsyncEnumerable<ChatResponsePart> StreamChatAsync(string modelName, List<Message> messages, CancellationToken cancellationToken = default)
        {
            ChatRequest request = new ChatRequest(modelName, messages);
            return StreamChatAsync(request, cancellationToken);
        }

        /// <summary>
        /// Streams chat responses for a list of messages with tool definitions.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="toolDefinitions">The list of tool definitions to include in the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        public IAsyncEnumerable<ChatResponsePart> StreamChatAsync(string modelName, List<Message> messages, IEnumerable<ToolDefinition> toolDefinitions, CancellationToken cancellationToken = default)
        {
            ChatRequest request = new ChatRequest(modelName, messages, toolDefinitions);
            return StreamChatAsync(request, cancellationToken);
        }

        /// <summary>
        /// Streams chat responses using a complete request object.
        /// </summary>
        /// <param name="request">The request containing model name, messages and optional tool definitions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        public async IAsyncEnumerable<ChatResponsePart> StreamChatAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await _mainClient.EnsureAuthorizedForSpecializedClientsAsync(cancellationToken);

            string json = request.ToJson();
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync("api/chat/stream", content, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token might be expired, but we can't easily retry with streaming
                // The main client should handle authorization before calling this
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Authentication failed. Status: {response.StatusCode}, Content: {errorContent}");
            }

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Failed to stream chat. Status: {response.StatusCode}, Content: {errorContent}");
            }

            Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using StreamReader reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (line.StartsWith("data: "))
                {
                    string data = line.Substring(6);

                    if (string.IsNullOrEmpty(data))
                        continue;

                    ChatResponsePart? responsePart = null;
                    Exception? parseException = null;

                    try
                    {
                        responsePart = ChatResponsePart.FromJson(data);
                    }
                    catch (Exception exception)
                    {
                        parseException = exception;
                    }

                    if (responsePart != null)
                    {
                        yield return responsePart;
                    }
                    else if (parseException != null)
                    {
                        // Try to parse as ExceptionResponse
                        try
                        {
                            ExceptionResponse exceptionResponse = ExceptionResponse.FromJson(data);
                            Exception recreatedException = exceptionResponse.RecreateException();
                            throw recreatedException;
                        }
                        catch
                        {
                            // If it's not an ExceptionResponse, rethrow the original exception
                            throw parseException;
                        }
                    }
                }
            }
        }
    }
}