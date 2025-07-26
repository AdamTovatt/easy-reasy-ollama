namespace EasyReasy.Ollama.Server.Providers
{
    /// <summary>
    /// Provides functionality to retrieve and validate allowed models.
    /// </summary>
    public interface IAllowedModelsProvider
    {
        /// <summary>
        /// Gets all allowed model names.
        /// </summary>
        /// <returns>An enumerable collection of allowed model names.</returns>
        IEnumerable<string> GetAllowedModels();

        /// <summary>
        /// Checks if a specific model name is allowed.
        /// </summary>
        /// <param name="modelName">The model name to check.</param>
        /// <returns>True if the model is allowed; otherwise, false.</returns>
        bool IsModelAllowed(string modelName);
    }
}
