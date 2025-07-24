using EasyReasy.EnvironmentVariables;

namespace EasyReasy.Ollama.Server
{
    [EnvironmentVariableNameContainer]
    public class EnvironmentVariables
    {
        [EnvironmentVariableName(5)]
        public static VariableName OllamaUrl = new VariableName("OLLAMA_URL");

        [EnvironmentVariableName(1)]
        public static VariableName OllamaModelName = new VariableName("OLLAMA_MODEL_NAME");

        [EnvironmentVariableName(32)]
        public static VariableName JwtSigningSecret = new VariableName("JWT_SIGNING_SECRET");
    }
}
