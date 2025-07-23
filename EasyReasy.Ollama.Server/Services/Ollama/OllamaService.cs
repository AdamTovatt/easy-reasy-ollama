using OllamaSharp;
using OllamaSharp.Models;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    public abstract class OllamaService : IOllamaService, IDisposable
    {
        protected const string _negativeKeepAliveValue = "-1s";
        protected OllamaApiClient _client;
        protected bool _keepModelLoaded;
        private bool _disposed;
        private ILogger<IOllamaService> _logger;

        protected OllamaService(string url, string model, bool keepModelLoaded, ILogger<IOllamaService> logger)
        {
            _client = new OllamaApiClient(url, model);
            _keepModelLoaded = keepModelLoaded;
            _logger = logger;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _client.Dispose();
            _disposed = true;
        }

        public async Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<Model> models = await _client.ListLocalModelsAsync(cancellationToken).ConfigureAwait(false);
            return models.Select(model => model.Name).ToList();
        }

        public async Task PullModelAsync(string modelName, CancellationToken cancellationToken = default)
        {
            PullModelRequest pullRequest = new PullModelRequest()
            {
                Model = modelName,
                Stream = true,
            };

            IAsyncEnumerable<PullModelResponse?> modelResponseStream = _client.PullModelAsync(pullRequest, cancellationToken);

            _logger.LogInformation($"Pulling model {modelName}");

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
                    _logger.LogInformation($"{modelName} ({shortDigest}): {percentString} ({response.Completed} / {response.Total})");
                }
                else
                {
                    _logger.LogInformation($"{response.Status} for {modelName}");
                }
            }
        }

        public async Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default)
        {
            List<string> availableModels = await GetAvailableModelsAsync(cancellationToken).ConfigureAwait(false);

            // Check if the modelName contains a version (e.g., ":latest" or ":[version]")
            int versionSeparatorIndex = modelName.IndexOf(':');
            if (versionSeparatorIndex > -1)
            {
                // Version specified, must match exactly
                foreach (string availableModel in availableModels)
                    if (string.Equals(availableModel, modelName, StringComparison.OrdinalIgnoreCase))
                        return true;
                return false;
            }
            else
            {
                // No version specified, match any model that starts with modelName + ":"
                string prefix = modelName + ":";
                foreach (string availableModel in availableModels)
                    if (availableModel.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        return true;
                return false;
            }
        }
    }
}