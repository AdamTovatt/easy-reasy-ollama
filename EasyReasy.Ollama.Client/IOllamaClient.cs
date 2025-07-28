namespace EasyReasy.Ollama.Client
{
    /// <summary>
    /// Interface for the main Ollama client that handles authentication and model management.
    /// </summary>
    public interface IOllamaClient : IDisposable
    {
        /// <summary>
        /// Gets the chat client for handling chat operations.
        /// </summary>
        IChatClient Chat { get; }

        /// <summary>
        /// Gets the embedding client for handling embedding operations.
        /// </summary>
        IEmbeddingClient Embeddings { get; }

        /// <summary>
        /// Gets a list of all available models that are currently downloaded on the Ollama server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of model names that are available locally.</returns>
        Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a specific model is available for use.
        /// </summary>
        /// <param name="modelName">The name of the model to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the model is available; otherwise, false.</returns>
        Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default);
    }
}