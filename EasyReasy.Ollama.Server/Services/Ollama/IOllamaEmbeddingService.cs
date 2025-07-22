namespace EasyReasy.Ollama.Server.Services.Ollama
{
    public interface IOllamaEmbeddingService : IDisposable
    {
        Task<float[]> GetEmbeddingsAsync(string text, CancellationToken cancellationToken = default);
    }
}