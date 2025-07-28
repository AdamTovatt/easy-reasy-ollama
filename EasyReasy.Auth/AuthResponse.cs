using System.Text.Json.Serialization;

namespace EasyReasy.Auth
{
    /// <summary>
    /// Response model for successful JWT token authentication.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// The JWT token for authentication.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        /// The expiration date/time of the token in ISO 8601 format (UTC).
        /// </summary>
        [JsonPropertyName("expiresAt")]
        public string ExpiresAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthResponse"/> class.
        /// </summary>
        /// <param name="token">The JWT token for authentication.</param>
        /// <param name="expiresAt">The expiration date/time of the token in ISO 8601 format (UTC).</param>
        public AuthResponse(string token, string expiresAt)
        {
            Token = token;
            ExpiresAt = expiresAt;
        }
    }
}