using System.Text.Json.Serialization;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Request model for username/password authentication.
    /// </summary>
    public class LoginAuthRequest
    {
        /// <summary>
        /// Gets the username for authentication.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; }

        /// <summary>
        /// Gets the password for authentication.
        /// </summary>
        [JsonPropertyName("password")]
        public string Password { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginAuthRequest"/> class.
        /// </summary>
        /// <param name="username">The username for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        public LoginAuthRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}