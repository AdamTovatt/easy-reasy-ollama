using OllamaSharp.Models;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    public class OllamaEmbeddingService : OllamaService, IOllamaEmbeddingService
    {
        private OllamaEmbeddingService(string url, string model, bool keepModelLoaded, ILogger<IOllamaService> logger)
            : base(url, model, keepModelLoaded, logger) { }

        public static OllamaEmbeddingService Create(string url, string model, ILogger<IOllamaService> logger, bool keepModelLoaded = true)
        {
            return new OllamaEmbeddingService(url, model, keepModelLoaded, logger);
        }

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