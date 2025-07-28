using EasyReasy.Ollama.Common;

namespace EasyReasy.Ollama.Client
{
    /// <summary>
    /// Interface for the embedding client that handles all embedding-related operations.
    /// </summary>
    public interface IEmbeddingClient
    {
        /// <summary>
        /// Gets embeddings for the given text using the specified model.
        /// </summary>
        /// <param name="modelName">The name of the model to use for embedding.</param>
        /// <param name="text">The text to embed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The embedding response containing the embedding vector.</returns>
        Task<EmbeddingResponse> GetEmbeddingsAsync(string modelName, string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets embeddings for the given text using the specified model.
        /// </summary>
        /// <param name="request">The request containing model name and text to embed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The embedding response containing the embedding vector.</returns>
        Task<EmbeddingResponse> GetEmbeddingsAsync(EmbeddingRequest request, CancellationToken cancellationToken = default);
    }
}