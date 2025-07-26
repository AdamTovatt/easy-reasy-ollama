using OllamaSharp.Models.Chat;
using System.Runtime.CompilerServices;
using EasyReasy.Ollama.Server.Providers;

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
        public async IAsyncEnumerable<string> GetResponseAsync(string text, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ChatRequest chatRequest = new ChatRequest();

            if (_keepModelLoaded)
                chatRequest.KeepAlive = _negativeKeepAliveValue;

            List<Message> messages = new List<Message>();
            messages.Add(new Message(ChatRole.User, text));
            chatRequest.Messages = messages;

            IAsyncEnumerable<ChatResponseStream?> result = _client.ChatAsync(chatRequest, cancellationToken);

            await foreach (ChatResponseStream? message in result)
            {
                if (message == null)
                    continue;

                yield return message.Message.Content ?? "";
            }
        }
    }
}