using System.Text.Json.Serialization;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Request model for username/password authentication.
    /// </summary>
    public class LoginAuthRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; }

        [JsonPropertyName("password")]
        public string Password { get; }

        public LoginAuthRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}