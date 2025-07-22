namespace EasyReasy.Ollama.Server.Services.Ollama
{
    public interface IOllamaService
    {
        IAsyncEnumerable<string> GetResponseAsync(string text, CancellationToken cancellationToken = default);
        Task<float[]> GetEmbeddingsAsync(string text, CancellationToken cancellationToken = default);
    }
}