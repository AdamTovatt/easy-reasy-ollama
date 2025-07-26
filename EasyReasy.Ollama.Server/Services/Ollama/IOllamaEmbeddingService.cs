namespace EasyReasy.Ollama.Server.Services.Ollama
{
    /// <summary>
    /// Interface for embedding services using Ollama models.
    /// </summary>
    public interface IOllamaEmbeddingService : IDisposable
    {
        /// <summary>
        /// Gets the embedding vector for the given input text.
        /// </summary>
        /// <param name="text">The input text to embed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A float array representing the embedding vector.</returns>
        Task<float[]> GetEmbeddingsAsync(string text, CancellationToken cancellationToken = default);
    }
}