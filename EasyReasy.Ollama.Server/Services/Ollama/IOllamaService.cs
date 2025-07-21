namespace EasyReasy.Ollama.Server.Services.Ollama
{
    public interface IOllamaService
    {
        IAsyncEnumerable<string> GetResponseAsync(string text, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<float>> GetEmbeddingsAsync(string text, CancellationToken cancellationToken = default);
    }
}
