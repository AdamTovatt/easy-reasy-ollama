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
        /// <param name="requireExactMatch">If an exact match is required or not. Defaults to true which is also the best option if checkig if a model is available. When just checking if a model is allowed it can be set to false.</param>
        /// <returns>True if the model is found; otherwise, false.</returns>
        public static bool IsModelFound(string modelName, IEnumerable<string> nameCollection, bool requireExactMatch = true)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return false;

            // Check if the modelName contains a version (e.g., ":latest" or ":[version]")
            int versionSeparatorIndex = modelName.IndexOf(':');
            if (versionSeparatorIndex > -1 && requireExactMatch)
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
                foreach (string availableModel in nameCollection)
                {
                    if (requireExactMatch || versionSeparatorIndex == -1)
                    {
                        if (availableModel.StartsWith(modelName, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    else
                    {
                        string firstPartOnly = modelName.Substring(0, versionSeparatorIndex);

                        if (availableModel.StartsWith(firstPartOnly, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
                return false;
            }
        }
    }
}
