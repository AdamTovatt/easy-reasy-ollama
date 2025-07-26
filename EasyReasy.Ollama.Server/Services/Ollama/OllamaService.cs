using OllamaSharp;
using OllamaSharp.Models;
using EasyReasy.Ollama.Server.Providers;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    /// <summary>
    /// Abstract base class for Ollama services that provides common functionality for interacting with Ollama API.
    /// </summary>
    public abstract class OllamaService : IDisposable
    {
        protected const string _negativeKeepAliveValue = "-1s";
        protected OllamaApiClient _client;
        protected bool _keepModelLoaded;
        private bool _disposed;
        protected ILogger _logger;
        private IAllowedModelsProvider _allowedModelsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OllamaService"/> class.
        /// </summary>
        /// <param name="url">The URL of the Ollama service.</param>
        /// <param name="model">The default model to use.</param>
        /// <param name="keepModelLoaded">Whether to keep the model loaded in memory.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="allowedModelsProvider">The provider for allowed models.</param>
        /// <exception cref="ArgumentException">Thrown when the specified default model is not allowed.</exception>
        protected OllamaService(string url, string model, bool keepModelLoaded, ILogger logger, IAllowedModelsProvider allowedModelsProvider)
        {
            if (!allowedModelsProvider.IsModelAllowed(model))
                throw new ArgumentException($"Model '{model}' is not allowed. Configure it in the environment variables of the server service if you think it should be allowed.", nameof(model));

            _client = new OllamaApiClient(url, model);
            _keepModelLoaded = keepModelLoaded;
            _logger = logger;
            _allowedModelsProvider = allowedModelsProvider;
        }

        /// <summary>
        /// Disposes of the Ollama service and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _client.Dispose();
            _disposed = true;
        }
    }
}