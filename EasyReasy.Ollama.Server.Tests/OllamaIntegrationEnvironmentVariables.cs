using EasyReasy.EnvironmentVariables;

namespace EasyReasy.Ollama.Server.Tests
{
    [EnvironmentVariableNameContainer]
    public static class OllamaIntegrationEnvironmentVariables
    {
        /// <summary>
        /// The URL of the Ollama service. Must be at least 5 characters long.
        /// </summary>
        [EnvironmentVariableName(5)]
        public static VariableName OllamaUrl = new VariableName("OLLAMA_URL");

        /// <summary>
        /// A range of environment variables that define allowed models. 
        /// Uses the prefix "ALLOWED_MODEL" and requires at least 1 model to be defined.
        /// Examples: ALLOWED_MODEL1, ALLOWED_MODEL2, ALLOWED_MODEL_DEV, etc.
        /// </summary>
        [EnvironmentVariableNameRange(minCount: 1)]
        public static VariableNameRange AllowedModels = new VariableNameRange("ALLOWED_MODEL");
    }
}