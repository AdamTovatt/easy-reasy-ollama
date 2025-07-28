using OllamaSharp.Models.Chat;
using System.Runtime.CompilerServices;
using EasyReasy.Ollama.Server.Providers;
using EasyReasy.Ollama.Server.Extensions;
using EasyReasy.Ollama.Common;
using Message = EasyReasy.Ollama.Common.Message;
using ChatRole = EasyReasy.Ollama.Common.ChatRole;
using System.Text;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    /// <summary>
    /// Service for handling chat interactions with Ollama models.
    /// </summary>
    public class OllamaChatService : OllamaService, IOllamaChatService
    {
        private static readonly JsonCleaner _jsonCleaner = new JsonCleaner("function", "parameters", "arguments");

        /// <summary>
        /// Initializes a new instance of the <see cref="OllamaChatService"/> class.
        /// </summary>
        /// <param name="url">The URL of the Ollama service.</param>
        /// <param name="model">The model to use for chat.</param>
        /// <param name="keepModelLoaded">Whether to keep the model loaded in memory.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="allowedModelsProvider">The provider for allowed models.</param>
        private OllamaChatService(string url, string model, bool keepModelLoaded, ILogger<OllamaChatService> logger, IAllowedModelsProvider allowedModelsProvider)
            : base(url, model, keepModelLoaded, logger, allowedModelsProvider) { }

        /// <summary>
        /// Creates a new instance of <see cref="OllamaChatService"/>.
        /// </summary>
        /// <param name="url">The URL of the Ollama service.</param>
        /// <param name="model">The model to use for chat.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="allowedModelsProvider">The provider for allowed models.</param>
        /// <param name="keepModelLoaded">Whether to keep the model loaded in memory. Default is true.</param>
        /// <returns>A new <see cref="OllamaChatService"/> instance.</returns>
        public static OllamaChatService Create(string url, string model, ILogger<OllamaChatService> logger, IAllowedModelsProvider allowedModelsProvider, bool keepModelLoaded = true)
        {
            return new OllamaChatService(url, model, keepModelLoaded, logger, allowedModelsProvider);
        }

        /// <summary>
        /// Gets a streaming chat response for the given user input text.
        /// </summary>
        /// <param name="text">The user input text to send to the model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response strings from the model.</returns>
        public IAsyncEnumerable<ChatResponsePart> GetResponseAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            List<Message> messages = new List<Message>
            {
                new Message(ChatRole.User, text)
            };

            return GetResponseAsync(messages, cancellationToken);
        }

        /// <summary>
        /// Gets a streaming chat response for the given list of messages.
        /// </summary>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response strings from the model.</returns>
        public IAsyncEnumerable<ChatResponsePart> GetResponseAsync(
            List<Message> messages,
            CancellationToken cancellationToken = default)
        {
            return GetResponseAsync(messages, null, cancellationToken);
        }

        /// <summary>
        /// Gets a streaming chat response for the given list of messages and tool calls.
        /// </summary>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="toolDefinitions">The list of tool definitions to include in the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response strings from the model.</returns>
        public async IAsyncEnumerable<ChatResponsePart> GetResponseAsync(
            List<Message> messages,
            IEnumerable<ToolDefinition>? toolDefinitions,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (messages == null || !messages.Any())
            {
                throw new ArgumentException("Messages cannot be null or empty", nameof(messages));
            }

            List<OllamaSharp.Models.Chat.Message> ollamaMessages = messages.ToOllamaSharp();

            ChatRequest chatRequest = new ChatRequest
            {
                Messages = ollamaMessages,
                Tools = toolDefinitions?.ToOllamaTools(),
            };

            if (_keepModelLoaded)
                chatRequest.KeepAlive = _negativeKeepAliveValue;

            IAsyncEnumerable<ChatResponseStream?> result = _client.ChatAsync(chatRequest, cancellationToken);

            StringBuilder? toolCallJsonBuilder = null;

            await foreach (ChatResponseStream? message in result.ConfigureAwait(false))
            {
                if (message == null)
                    continue;

                // Check if this message contains tool calls
                if (message.Message.ToolCalls != null && message.Message.ToolCalls.Any())
                {
                    // Start collecting tool call JSON
                    if (toolCallJsonBuilder == null)
                    {
                        string? toolName = message.Message.ToolCalls.First().Function?.Name;

                        if (toolName == null)
                            throw new Exception($"Model failed to specify tool name in a tool call");

                        // Add the start of the json before
                        toolCallJsonBuilder = new StringBuilder($"{{\"function\":{{\"index\":null,\"name\":\"{toolName}");
                    }

                    // Add the tool call content to our builder
                    if (!string.IsNullOrEmpty(message.Message.Content))
                    {
                        toolCallJsonBuilder.Append(message.Message.Content);
                    }
                }
                else if (message.Message.Content != null)
                {
                    if (toolCallJsonBuilder != null)
                    {
                        toolCallJsonBuilder.Append(message.Message.Content);

                        if (message.Done)
                        {
                            // Add closing "}" after the json since we added a start json snippet containing "{"
                            string cleanedJson = _jsonCleaner.CleanJson($"{toolCallJsonBuilder}}}");
                            ToolCall? toolCall = ToolCall.FromJson(cleanedJson, throwOnError: false);

                            if (toolCall == null)
                                throw new Exception($"Model provided invalid tool call json: {toolCallJsonBuilder}");

                            yield return new ChatResponsePart(toolCall);
                        }
                    }
                    else
                    {
                        // Return normal content
                        yield return new ChatResponsePart(message.Message.Content);
                    }
                }
            }
        }
    }
}