namespace EasyReasy.Ollama.Server.Services.Ollama
{
    /// <summary>
    /// Factory for creating and managing Ollama service instances for different models.
    /// </summary>
    public interface IOllamaServiceFactory
    {
        /// <summary>
        /// Gets a chat service for the specified model. Creates a new instance if one doesn't exist.
        /// </summary>
        /// <param name="modelName">The name of the model to use for chat.</param>
        /// <returns>A chat service instance for the specified model.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified model is not allowed.</exception>
        IOllamaChatService GetChatService(string modelName);

        /// <summary>
        /// Gets an embedding service for the specified model. Creates a new instance if one doesn't exist.
        /// </summary>
        /// <param name="modelName">The name of the model to use for embeddings.</param>
        /// <returns>An embedding service instance for the specified model.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified model is not allowed.</exception>
        IOllamaEmbeddingService GetEmbeddingService(string modelName);

        /// <summary>
        /// Gets a list of all available models that are currently downloaded on the Ollama server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of model names that are available locally.</returns>
        Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a specific model from the Ollama registry with progress logging.
        /// </summary>
        /// <param name="modelName">The name of the model to download.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous download operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified model is not allowed.</exception>
        Task PullModelAsync(string modelName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a specific model is available for use.
        /// Handles both exact model names and base model names with version matching.
        /// </summary>
        /// <param name="modelName">The name of the model to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the model is available; otherwise, false.</returns>
        Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads all allowed models that are not currently available.
        /// Uses the allowed models provider to determine which models should be downloaded.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous download operation.</returns>
        Task PullModelsAsync(CancellationToken cancellationToken = default);
    }
}