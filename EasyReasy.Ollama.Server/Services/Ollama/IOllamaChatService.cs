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
        IAsyncEnumerable<string> GetResponseAsync(string text, CancellationToken cancellationToken = default);
    }
}