using System.Text.Json.Serialization;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Request model for API key authentication.
    /// </summary>
    public class ApiKeyAuthRequest
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; }

        public ApiKeyAuthRequest(string apiKey)
        {
            ApiKey = apiKey;
        }
    }
}