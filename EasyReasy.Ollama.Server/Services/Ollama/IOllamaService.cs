namespace EasyReasy.Ollama.Server.Services.Ollama
{
    public interface IOllamaService
    {
        Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
        Task PullModelAsync(string modelName, CancellationToken cancellationToken = default);
        Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default);
    }
}