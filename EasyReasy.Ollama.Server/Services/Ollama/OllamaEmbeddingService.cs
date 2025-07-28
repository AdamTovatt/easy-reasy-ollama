using EasyReasy.Ollama.Server.Providers;
using OllamaSharp.Models;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    /// <summary>
    /// Service for generating embeddings using Ollama models.
    /// </summary>
    public class OllamaEmbeddingService : OllamaService, IOllamaEmbeddingService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OllamaEmbeddingService"/> class.
        /// </summary>
        /// <param name="url">The URL of the Ollama service.</param>
        /// <param name="model">The model to use for embeddings.</param>
        /// <param name="keepModelLoaded">Whether to keep the model loaded in memory.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="allowedModelsProvider">The provider for allowed models.</param>
        private OllamaEmbeddingService(string url, string model, bool keepModelLoaded, ILogger<OllamaEmbeddingService> logger, IAllowedModelsProvider allowedModelsProvider)
            : base(url, model, keepModelLoaded, logger, allowedModelsProvider) { }

        /// <summary>
        /// Creates a new instance of <see cref="OllamaEmbeddingService"/>.
        /// </summary>
        /// <param name="url">The URL of the Ollama service.</param>
        /// <param name="model">The model to use for embeddings.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="allowedModelsProvider">The provider for allowed models.</param>
        /// <param name="keepModelLoaded">Whether to keep the model loaded in memory. Default is true.</param>
        /// <returns>A new <see cref="OllamaEmbeddingService"/> instance.</returns>
        public static OllamaEmbeddingService Create(string url, string model, ILogger<OllamaEmbeddingService> logger, IAllowedModelsProvider allowedModelsProvider, bool keepModelLoaded = true)
        {
            return new OllamaEmbeddingService(url, model, keepModelLoaded, logger, allowedModelsProvider);
        }

        /// <summary>
        /// Gets the embedding vector for the given input text.
        /// </summary>
        /// <param name="text">The input text to embed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A float array representing the embedding vector.</returns>
        public async Task<float[]> GetEmbeddingsAsync(string text, CancellationToken cancellationToken = default)
        {
            EmbedRequest embedRequest = new EmbedRequest
            {
                Model = _client.SelectedModel,
                Input = new List<string>() { text },
            };

            if (_keepModelLoaded)
                embedRequest.KeepAlive = _negativeKeepAliveValue;

            EmbedResponse embedResponse = await _client.EmbedAsync(embedRequest, cancellationToken);

            if (embedResponse == null || embedResponse.Embeddings == null || embedResponse.Embeddings.Count == 0)
                return Array.Empty<float>();

            return embedResponse.Embeddings.First();
        }
    }
}