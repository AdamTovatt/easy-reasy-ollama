namespace EasyReasy.Ollama.Server.Helpers
{
    /// <summary>
    /// Provides utility methods for matching model names against collections of available models.
    /// </summary>
    public class ModelNameMatcher
    {
        /// <summary>
        /// Checks if a model name is found in a collection of available model names.
        /// Handles both exact model names and base model names with version matching.
        /// </summary>
        /// <param name="modelName">The name of the model to check.</param>
        /// <param name="nameCollection">The collection of available model names to search in.</param>
        /// <returns>True if the model is found; otherwise, false.</returns>
        public static bool IsModelFound(string modelName, IEnumerable<string> nameCollection)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return false;

            // Check if the modelName contains a version (e.g., ":latest" or ":[version]")
            int versionSeparatorIndex = modelName.IndexOf(':');
            if (versionSeparatorIndex > -1)
            {
                // Version specified, must match exactly
                foreach (string availableModel in nameCollection)
                    if (string.Equals(availableModel, modelName, StringComparison.OrdinalIgnoreCase))
                        return true;
                return false;
            }
            else
            {
                // No version specified, match any model that starts with modelName + ":"
                string prefix = modelName;
                foreach (string availableModel in nameCollection)
                    if (availableModel.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        return true;
                return false;
            }
        }
    }
}
