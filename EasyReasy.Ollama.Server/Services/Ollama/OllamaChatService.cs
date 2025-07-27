using OllamaSharp.Models.Chat;
using System.Runtime.CompilerServices;
using EasyReasy.Ollama.Server.Providers;
using EasyReasy.Ollama.Server.Extensions;
using EasyReasy.Ollama.Common;
using Message = EasyReasy.Ollama.Common.Message;
using ChatRole = EasyReasy.Ollama.Common.ChatRole;
using OllamaSharp;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    /// <summary>
    /// Service for handling chat interactions with Ollama models.
    /// </summary>
    public class OllamaChatService : OllamaService, IOllamaChatService
    {
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

            await foreach (ChatResponseStream? message in result.ConfigureAwait(false))
            {
                if (message == null)
                    continue;

                if (message.Message.ToolCalls != null && message.Message.ToolCalls.Any())
                {
                    yield return new ChatResponsePart(message.Message.ToolCalls.ToCommon());
                }
                else if (message.Message.Content != null)
                {
                    yield return new ChatResponsePart(message.Message.Content);
                }
            }
        }
    }
}