using EasyReasy.EnvironmentVariables;

namespace EasyReasy.Ollama.Client.Tests
{
    [EnvironmentVariableNameContainer]
    public static class RemoteIntegrationEnvironmentVariables
    {
        /// <summary>
        /// The URL of the remote Ollama service. Must be at least 5 characters long.
        /// </summary>
        [EnvironmentVariableName(minLength: 5)]
        public static readonly VariableName RemoteUrl = new VariableName("REMOTE_OLLAMA_URL");

        /// <summary>
        /// The API key for authentication with the remote Ollama service. Must be at least 10 characters long.
        /// </summary>
        [EnvironmentVariableName(minLength: 10)]
        public static readonly VariableName ApiKey = new VariableName("REMOTE_OLLAMA_API_KEY");
    }
}
