using EasyReasy.Auth.Client;

namespace EasyReasy.Ollama.Client
{
    /// <summary>
    /// Implementation of the main Ollama client that handles authentication and model management.
    /// </summary>
    public class OllamaClient : IOllamaClient
    {
        private readonly AuthorizedHttpClient _authorizedHttpClient;
        private bool _disposed;

        /// <summary>
        /// Gets the chat client for handling chat operations.
        /// </summary>
        public IChatClient Chat { get; }

        /// <summary>
        /// Gets the embedding client for handling embedding operations.
        /// </summary>
        public IEmbeddingClient Embeddings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OllamaClient"/> class.
        /// </summary>
        /// <param name="authorizedHttpClient">The authorized HTTP client to use for requests.</param>
        private OllamaClient(AuthorizedHttpClient authorizedHttpClient)
        {
            _authorizedHttpClient = authorizedHttpClient ?? throw new ArgumentNullException(nameof(authorizedHttpClient));

            Chat = new ChatClient(authorizedHttpClient.HttpClient, this);
            Embeddings = new EmbeddingClient(authorizedHttpClient.HttpClient, this);
        }

        /// <summary>
        /// Ensures the client is authorized before making API calls.
        /// This method should be called by specialized clients before making requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        internal async Task EnsureAuthorizedForSpecializedClientsAsync(CancellationToken cancellationToken = default)
        {
            await _authorizedHttpClient.EnsureAuthorizedAsync(cancellationToken);
        }

        /// <summary>
        /// Creates an Ollama client using an API key without automatically authorizing.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests.</param>
        /// <param name="apiKey">The API key for authentication.</param>
        /// <returns>An Ollama client that needs to be authorized before use.</returns>
        public static OllamaClient CreateUnauthorized(HttpClient httpClient, string apiKey)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));
            }

            AuthorizedHttpClient authorizedHttpClient = new AuthorizedHttpClient(httpClient, apiKey);
            return new OllamaClient(authorizedHttpClient);
        }

        /// <summary>
        /// Creates an authenticated Ollama client using an API key.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests.</param>
        /// <param name="apiKey">The API key for authentication.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An authenticated Ollama client.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when authentication fails.</exception>
        public static async Task<OllamaClient> CreateAuthorizedAsync(HttpClient httpClient, string apiKey, CancellationToken cancellationToken = default)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));
            }

            AuthorizedHttpClient authorizedHttpClient = new AuthorizedHttpClient(httpClient, apiKey);
            await authorizedHttpClient.EnsureAuthorizedAsync(cancellationToken);

            return new OllamaClient(authorizedHttpClient);
        }

        /// <summary>
        /// Gets a list of all available models that are currently downloaded on the Ollama server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of model names that are available locally.</returns>
        public async Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _authorizedHttpClient.GetAsync("/api/ollama/models", cancellationToken);

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
            HttpResponseMessage response = await _authorizedHttpClient.GetAsync($"/api/ollama/models/{Uri.EscapeDataString(modelName)}/available", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to check model availability. Status: {response.StatusCode}");
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            bool isAvailable = System.Text.Json.JsonSerializer.Deserialize<bool>(json);

            return isAvailable;
        }

        /// <summary>
        /// Disposes of the client. Note that the HttpClient is not disposed as it's managed externally.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _authorizedHttpClient.Dispose();
                _disposed = true;
            }
        }
    }
}