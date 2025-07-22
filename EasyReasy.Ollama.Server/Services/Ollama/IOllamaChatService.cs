namespace EasyReasy.Ollama.Server.Services.Ollama
{
    public interface IOllamaChatService : IDisposable
    {
        IAsyncEnumerable<string> GetResponseAsync(string text, CancellationToken cancellationToken = default);
    }
}