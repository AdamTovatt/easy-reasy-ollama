using EasyReasy.Auth;
using EasyReasy.Ollama.Common;

namespace EasyReasy.Ollama.Client
{
    /// <summary>
    /// Implementation of the main Ollama client that handles authentication and model management.
    /// </summary>
    public class OllamaClient : IOllamaClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private bool _disposed;
        private bool _isAuthorized;
        private DateTime? _tokenExpiresAt;

        /// <summary>
        /// Gets the chat client for handling chat operations.
        /// </summary>
        public IChatClient Chat { get; }

        /// <summary>
        /// Gets the embedding client for handling embedding operations.
        /// </summary>
        public IEmbeddingClient Embeddings { get; }

        /// <summary>
        /// Ensures the client is authorized before making API calls.
        /// This method should be called by specialized clients before making requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        internal async Task EnsureAuthorizedForSpecializedClientsAsync(CancellationToken cancellationToken = default)
        {
            await EnsureAuthorizedAsync(cancellationToken);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OllamaClient"/> class.
        /// </summary>
        /// <param name="baseUrl">The base URL of the Ollama server.</param>
        /// <param name="apiKey">The API key for authentication.</param>
        private OllamaClient(string baseUrl, string apiKey)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _apiKey = apiKey;
            _httpClient = new HttpClient();

            Chat = new ChatClient(_httpClient, _baseUrl, this);
            Embeddings = new EmbeddingClient(_httpClient, _baseUrl, this);
        }

        /// <summary>
        /// Ensures the client is authorized and the token is not expired.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task EnsureAuthorizedAsync(CancellationToken cancellationToken = default)
        {
            // Check if we need to authorize or re-authorize
            if (!_isAuthorized || IsTokenExpired())
            {
                await AuthorizeAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Checks if the current token is expired.
        /// </summary>
        /// <returns>True if the token is expired or will expire within 5 minutes; otherwise, false.</returns>
        private bool IsTokenExpired()
        {
            if (!_tokenExpiresAt.HasValue)
                return true;

            // Consider token expired if it expires within 5 minutes
            return _tokenExpiresAt.Value <= DateTime.UtcNow.AddMinutes(5);
        }

        /// <summary>
        /// Authorizes the client using the API key and obtains a JWT token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="HttpRequestException">Thrown when authentication fails.</exception>
        private async Task AuthorizeAsync(CancellationToken cancellationToken = default)
        {
            ApiKeyAuthRequest authRequest = new ApiKeyAuthRequest(_apiKey);
            string json = authRequest.ToJson();
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/api/auth/apikey", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Authentication failed. Status: {response.StatusCode}, Content: {errorContent}");
            }

            string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            AuthResponse authResponse = AuthResponse.FromJson(responseJson);

            // Set the authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);
            _isAuthorized = true;
            _tokenExpiresAt = DateTime.Parse(authResponse.ExpiresAt);
        }

        /// <summary>
        /// Creates an Ollama client using an API key without automatically authorizing.
        /// </summary>
        /// <param name="baseUrl">The base URL of the Ollama server.</param>
        /// <param name="apiKey">The API key for authentication.</param>
        /// <returns>An Ollama client that needs to be authorized before use.</returns>
        public static OllamaClient CreateAsync(string baseUrl, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));
            }

            return new OllamaClient(baseUrl, apiKey);
        }

        /// <summary>
        /// Creates an authenticated Ollama client using an API key.
        /// </summary>
        /// <param name="baseUrl">The base URL of the Ollama server.</param>
        /// <param name="apiKey">The API key for authentication.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An authenticated Ollama client.</returns>
        /// <exception cref="HttpRequestException">Thrown when authentication fails.</exception>
        public static async Task<OllamaClient> CreateAuthorizedAsync(string baseUrl, string apiKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));
            }

            OllamaClient client = new OllamaClient(baseUrl, apiKey);
            await client.AuthorizeAsync(cancellationToken);
            return client;
        }

        /// <summary>
        /// Gets a list of all available models that are currently downloaded on the Ollama server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of model names that are available locally.</returns>
        public async Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
        {
            await EnsureAuthorizedAsync(cancellationToken);

            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}/api/ollama/models", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token might be expired, try to re-authorize once
                await AuthorizeAsync(cancellationToken);
                response = await _httpClient.GetAsync($"{_baseUrl}/api/ollama/models", cancellationToken);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to get available models. Status: {response.StatusCode}");
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            List<string> models = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

            return models;
        }

        /// <summary>
        /// Checks if a specific model is available for use.
        /// </summary>
        /// <param name="modelName">The name of the model to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the model is available; otherwise, false.</returns>
        public async Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default)
        {
            await EnsureAuthorizedAsync(cancellationToken);

            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}/api/ollama/models/{Uri.EscapeDataString(modelName)}/available", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token might be expired, try to re-authorize once
                await AuthorizeAsync(cancellationToken);
                response = await _httpClient.GetAsync($"{_baseUrl}/api/ollama/models/{Uri.EscapeDataString(modelName)}/available", cancellationToken);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to check model availability. Status: {response.StatusCode}");
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            bool isAvailable = System.Text.Json.JsonSerializer.Deserialize<bool>(json);

            return isAvailable;
        }

        /// <summary>
        /// Disposes of the HTTP client and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient.Dispose();
                _disposed = true;
            }
        }
    }
}