using OllamaSharp.Models.Chat;
using System.Runtime.CompilerServices;
using EasyReasy.Ollama.Server.Providers;
using EasyReasy.Ollama.Server.Extensions;
using EasyReasy.Ollama.Common;
using Message = EasyReasy.Ollama.Common.Message;
using ChatRole = EasyReasy.Ollama.Common.ChatRole;

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
        public async IAsyncEnumerable<string> GetResponseAsync(
            string text,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            List<Message> messages = new List<Message>
            {
                new Message(ChatRole.User, text)
            };

            await foreach (string response in GetResponseAsync(messages, cancellationToken))
            {
                yield return response;
            }
        }

        /// <summary>
        /// Gets a streaming chat response for the given list of messages.
        /// </summary>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response strings from the model.</returns>
        public async IAsyncEnumerable<string> GetResponseAsync(
            List<Message> messages,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (string response in GetResponseAsync(messages, null, cancellationToken))
            {
                yield return response;
            }
        }

        /// <summary>
        /// Gets a streaming chat response for the given list of messages and tool calls.
        /// </summary>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="toolCalls">The list of tool calls to include in the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response strings from the model.</returns>
        public async IAsyncEnumerable<string> GetResponseAsync(
            List<Message> messages,
            IEnumerable<ToolCall>? toolCalls,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (messages == null || !messages.Any())
            {
                throw new ArgumentException("Messages cannot be null or empty", nameof(messages));
            }

            List<OllamaSharp.Models.Chat.Message> ollamaMessages = messages.ToOllamaSharp().ToList();

            ChatRequest chatRequest = new ChatRequest
            {
                Messages = ollamaMessages
            };

            if (_keepModelLoaded)
                chatRequest.KeepAlive = _negativeKeepAliveValue;

            IAsyncEnumerable<ChatResponseStream?> result = _client.ChatAsync(chatRequest, cancellationToken);

            await foreach (ChatResponseStream? message in result.ConfigureAwait(false))
            {
                if (message == null || message.Message.Content == null)
                    continue;

                yield return message.Message.Content;
            }
        }


    }
}