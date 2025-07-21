using EasyReasy.EnvironmentVariables;

namespace EasyReasy.Ollama.Server
{
    [EnvironmentVariableNameContainer]
    public class EnvironmentVariable
    {
        [EnvironmentVariableName(5)]
        public static string OllamaUrl = "OLLAMA_URL";

        [EnvironmentVariableName(1)]
        public static string OllamaModelName = "OLLAMA_MODEL_NAME";

        [EnvironmentVariableName(32)]
        public static string JwtSigningSecret = "JWT_SIGNING_SECRET";
    }
}
