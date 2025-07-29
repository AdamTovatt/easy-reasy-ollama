using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Server.Helpers;
using System.Collections.Concurrent;

namespace EasyReasy.Ollama.Server.Providers
{
    /// <summary>
    /// Implementation of <see cref="IAllowedModelsProvider"/> that retrieves allowed models from environment variables.
    /// Uses environment variables with the prefix "ALLOWED_MODEL" to determine which models are allowed.
    /// </summary>
    public class EnvironmentVariablesAllowedModelsProvider : IAllowedModelsProvider
    {
        private readonly Lazy<IEnumerable<string>> _allowedModelsCache;
        private readonly ConcurrentDictionary<string, bool> _modelAllowedCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariablesAllowedModelsProvider"/> class.
        /// </summary>
        public EnvironmentVariablesAllowedModelsProvider()
        {
            _allowedModelsCache = new Lazy<IEnumerable<string>>(() => EnvironmentVariables.AllowedModels.GetAllValues(), isThreadSafe: true);
            _modelAllowedCache = new ConcurrentDictionary<string, bool>();
        }

        /// <summary>
        /// Gets all allowed model names from environment variables with the "ALLOWED_MODEL" prefix.
        /// Results are cached for performance.
        /// </summary>
        /// <returns>An enumerable collection of allowed model names from environment variables.</returns>
        public IEnumerable<string> GetAllowedModels()
        {
            return _allowedModelsCache.Value;
        }

        /// <summary>
        /// Checks if a specific model name is allowed by comparing it against the allowed models from environment variables.
        /// The comparison is case-insensitive. Results are cached for performance.
        /// </summary>
        /// <param name="modelName">The model name to check.</param>
        /// <returns>True if the model is allowed; otherwise, false. Returns false for null or whitespace model names.</returns>
        public bool IsModelAllowed(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                return false;
            }

            return _modelAllowedCache.GetOrAdd(modelName, CheckIfModelAllowed);
        }

        /// <summary>
        /// Clears the cache of model allowed checks.
        /// </summary>
        public void ClearCache()
        {
            _modelAllowedCache.Clear();
        }

        private bool CheckIfModelAllowed(string modelName)
        {
            IEnumerable<string> allowedModels = GetAllowedModels();
            return ModelNameMatcher.IsModelFound(modelName, allowedModels, requireExactMatch: false);
        }
    }
}