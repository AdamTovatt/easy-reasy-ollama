using EasyReasy.Ollama.Server.Helpers;
using EasyReasy.Ollama.Server.Providers;
using System.Collections.Concurrent;
using OllamaSharp;
using OllamaSharp.Models;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    /// <summary>
    /// Factory implementation for creating and managing Ollama service instances for different models.
    /// Caches service instances to avoid recreating them for the same model.
    /// </summary>
    public class OllamaServiceFactory : IOllamaServiceFactory, IDisposable
    {
        private readonly string _ollamaUrl;
        private readonly ILogger<OllamaChatService> _chatLogger;
        private readonly ILogger<OllamaEmbeddingService> _embeddingLogger;
        private readonly IAllowedModelsProvider _allowedModelsProvider;
        private readonly ConcurrentDictionary<string, IOllamaChatService> _chatServices;
        private readonly ConcurrentDictionary<string, IOllamaEmbeddingService> _embeddingServices;
        private readonly OllamaApiClient _sharedClient;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OllamaServiceFactory"/> class.
        /// </summary>
        /// <param name="ollamaUrl">The URL of the Ollama service.</param>
        /// <param name="chatLogger">The logger instance for chat services.</param>
        /// <param name="embeddingLogger">The logger instance for embedding services.</param>
        /// <param name="allowedModelsProvider">The provider for allowed models.</param>
        public OllamaServiceFactory(string ollamaUrl, ILogger<OllamaChatService> chatLogger, ILogger<OllamaEmbeddingService> embeddingLogger, IAllowedModelsProvider allowedModelsProvider)
        {
            _ollamaUrl = ollamaUrl;
            _chatLogger = chatLogger;
            _embeddingLogger = embeddingLogger;
            _allowedModelsProvider = allowedModelsProvider;
            _chatServices = new ConcurrentDictionary<string, IOllamaChatService>();
            _embeddingServices = new ConcurrentDictionary<string, IOllamaEmbeddingService>();
            _sharedClient = new OllamaApiClient(ollamaUrl);
        }

        /// <summary>
        /// Gets a chat service for the specified model. Creates a new instance if one doesn't exist.
        /// </summary>
        /// <param name="modelName">The name of the model to use for chat.</param>
        /// <returns>A chat service instance for the specified model.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified model is not allowed.</exception>
        public IOllamaChatService GetChatService(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be null or whitespace.", nameof(modelName));

            if (!_allowedModelsProvider.IsModelAllowed(modelName))
                throw new ArgumentException($"Model '{modelName}' is not allowed. Configure it in the environment variables of the server service if you think it should be allowed.", nameof(modelName));

            return _chatServices.GetOrAdd(modelName, CreateChatService);
        }

        /// <summary>
        /// Gets an embedding service for the specified model. Creates a new instance if one doesn't exist.
        /// </summary>
        /// <param name="modelName">The name of the model to use for embeddings.</param>
        /// <returns>An embedding service instance for the specified model.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified model is not allowed.</exception>
        public IOllamaEmbeddingService GetEmbeddingService(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be null or whitespace.", nameof(modelName));

            if (!_allowedModelsProvider.IsModelAllowed(modelName))
                throw new ArgumentException($"Model '{modelName}' is not allowed. Configure it in the environment variables of the server service if you think it should be allowed.", nameof(modelName));

            return _embeddingServices.GetOrAdd(modelName, CreateEmbeddingService);
        }

        /// <summary>
        /// Gets a list of all available models that are currently downloaded on the Ollama server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of model names that are available locally.</returns>
        public async Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<Model> models = await _sharedClient.ListLocalModelsAsync(cancellationToken).ConfigureAwait(false);
            return models.Select(model => model.Name).ToList();
        }

        /// <summary>
        /// Downloads a specific model from the Ollama registry with progress logging.
        /// </summary>
        /// <param name="modelName">The name of the model to download.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous download operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified model is not allowed.</exception>
        public async Task PullModelAsync(string modelName, CancellationToken cancellationToken = default)
        {
            if (!_allowedModelsProvider.IsModelAllowed(modelName))
                throw new ArgumentException($"Model '{modelName}' is not allowed. Configure it in the environment variables of the server service if you think it should be allowed.", nameof(modelName));

            PullModelRequest pullRequest = new PullModelRequest()
            {
                Model = modelName,
                Stream = true,
            };

            IAsyncEnumerable<PullModelResponse?> modelResponseStream = _sharedClient.PullModelAsync(pullRequest, cancellationToken);

            _chatLogger.LogInformation($"Pulling model {modelName}");

            int lastPercent = -1;
            await foreach (PullModelResponse? response in modelResponseStream)
            {
                if (response == null)
                    continue;

                if (!string.IsNullOrEmpty(response.Status) && response.Status.Any(c => char.IsNumber(c)))
                {
                    int intPercentage = (int)response.Percent;

                    if (intPercentage == lastPercent)
                        continue;

                    string shortDigest = response.Digest;
                    if (!string.IsNullOrEmpty(shortDigest) && shortDigest.StartsWith("sha256:") && shortDigest.Length > 15)
                        shortDigest = shortDigest.Substring(7, 12);

                    // Format percent as right-aligned, space-padded 3-character string (e.g., ' 0%', '10%', '99%')
                    string percentString = intPercentage.ToString().PadLeft(2) + "%";
                    _chatLogger.LogInformation($"{modelName} ({shortDigest}): {percentString} ({response.Completed} / {response.Total})");
                }
                else
                {
                    _chatLogger.LogInformation($"{response.Status} for {modelName}");
                }
            }
        }

        /// <summary>
        /// Checks if a specific model is available for use.
        /// Handles both exact model names and base model names with version matching.
        /// </summary>
        /// <param name="modelName">The name of the model to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the model is available; otherwise, false.</returns>
        public async Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default)
        {
            List<string> availableModels = await GetAvailableModelsAsync(cancellationToken).ConfigureAwait(false);
            return ModelNameMatcher.IsModelFound(modelName, availableModels);
        }

        /// <summary>
        /// Downloads all allowed models that are not currently available.
        /// Uses the allowed models provider to determine which models should be downloaded.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous download operation.</returns>
        public async Task PullModelsAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<string> allowedModels = _allowedModelsProvider.GetAllowedModels();

            foreach (string allowedModel in allowedModels)
            {
                bool isAvailable = await IsModelAvailableAsync(allowedModel, cancellationToken).ConfigureAwait(false);
                
                if (!isAvailable)
                {
                    await PullModelAsync(allowedModel, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Disposes of all cached service instances and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            foreach (IOllamaChatService service in _chatServices.Values)
            {
                service.Dispose();
            }
            _chatServices.Clear();

            foreach (IOllamaEmbeddingService service in _embeddingServices.Values)
            {
                service.Dispose();
            }
            _embeddingServices.Clear();

            _sharedClient.Dispose();
            _disposed = true;
        }

        private IOllamaChatService CreateChatService(string modelName)
        {
            _chatLogger.LogInformation($"Creating new chat service for model: {modelName}");
            return OllamaChatService.Create(_ollamaUrl, modelName, _chatLogger, _allowedModelsProvider);
        }

        private IOllamaEmbeddingService CreateEmbeddingService(string modelName)
        {
            _embeddingLogger.LogInformation($"Creating new embedding service for model: {modelName}");
            return OllamaEmbeddingService.Create(_ollamaUrl, modelName, _embeddingLogger, _allowedModelsProvider);
        }
    }
} 