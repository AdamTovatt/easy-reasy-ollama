using EasyReasy.Ollama.Common;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    /// <summary>
    /// Interface for chat services using Ollama models.
    /// </summary>
    public interface IOllamaChatService : IDisposable
    {
        /// <summary>
        /// Gets a streaming chat response for the given user input text.
        /// </summary>
        /// <param name="text">The user input text to send to the model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response strings from the model.</returns>
        IAsyncEnumerable<ChatResponsePart> GetResponseAsync(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a streaming chat response for the given list of messages.
        /// </summary>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response strings from the model.</returns>
        IAsyncEnumerable<ChatResponsePart> GetResponseAsync(List<Message> messages, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a streaming chat response for the given list of messages and tool calls.
        /// </summary>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="toolDefinitions">The list of tool definitions to include in the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response strings from the model.</returns>
        IAsyncEnumerable<ChatResponsePart> GetResponseAsync(List<Message> messages, IEnumerable<ToolDefinition>? toolDefinitions, CancellationToken cancellationToken = default);
    }
}