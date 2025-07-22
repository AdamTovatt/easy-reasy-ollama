using EasyReasy.EnvironmentVariables;

namespace EasyReasy.Ollama.Server.Tests
{
    [EnvironmentVariableNameContainer]
    public static class OllamaIntegrationEnvironmentVariables
    {
        [EnvironmentVariableName(5)]
        public static string OllamaUrl = "OLLAMA_URL";

        [EnvironmentVariableName(1)]
        public static string OllamaModelName = "OLLAMA_MODEL_NAME";
    }
} 