using EasyReasy.Ollama.Common;

namespace EasyReasy.Ollama.Client
{
    /// <summary>
    /// Interface for the chat client that handles all chat-related operations.
    /// </summary>
    public interface IChatClient
    {
        /// <summary>
        /// Streams chat responses for a single text input.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="text">The user input text.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        IAsyncEnumerable<ChatResponsePart> StreamChatAsync(string modelName, string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams chat responses for a list of messages.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        IAsyncEnumerable<ChatResponsePart> StreamChatAsync(string modelName, List<Message> messages, CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams chat responses for a list of messages with tool definitions.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="toolDefinitions">The list of tool definitions to include in the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        IAsyncEnumerable<ChatResponsePart> StreamChatAsync(string modelName, List<Message> messages, IEnumerable<ToolDefinition> toolDefinitions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams chat responses using a complete request object.
        /// </summary>
        /// <param name="request">The request containing model name, messages and optional tool definitions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        IAsyncEnumerable<ChatResponsePart> StreamChatAsync(ChatRequest request, CancellationToken cancellationToken = default);
    }
}