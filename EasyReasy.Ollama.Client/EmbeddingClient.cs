using EasyReasy.Ollama.Common;

namespace EasyReasy.Ollama.Client
{
    /// <summary>
    /// Implementation of the embedding client that handles all embedding-related operations.
    /// </summary>
    public class EmbeddingClient : IEmbeddingClient
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaClient _mainClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddingClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use.</param>
        /// <param name="mainClient">The main Ollama client for authorization.</param>
        public EmbeddingClient(HttpClient httpClient, OllamaClient mainClient)
        {
            _httpClient = httpClient;
            _mainClient = mainClient;
        }

        /// <summary>
        /// Gets embeddings for the given text using the specified model.
        /// </summary>
        /// <param name="modelName">The name of the model to use for embedding.</param>
        /// <param name="text">The text to embed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The embedding response containing the embedding vector.</returns>
        public async Task<EmbeddingResponse> GetEmbeddingsAsync(string modelName, string text, CancellationToken cancellationToken = default)
        {
            EmbeddingRequest request = new EmbeddingRequest(modelName, text);
            return await GetEmbeddingsAsync(request, cancellationToken);
        }

        /// <summary>
        /// Gets embeddings for the given text using the specified model.
        /// </summary>
        /// <param name="request">The request containing model name and text to embed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The embedding response containing the embedding vector.</returns>
        public async Task<EmbeddingResponse> GetEmbeddingsAsync(EmbeddingRequest request, CancellationToken cancellationToken = default)
        {
            await _mainClient.EnsureAuthorizedForSpecializedClientsAsync(cancellationToken);

            string json = request.ToJson();
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool retried = false;

            while (true)
            {
                response = await _httpClient.PostAsync("api/embeddings", content, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !retried)
                {
                    // Token might be expired, force refresh and retry once
                    retried = true;
                    
                    // Dispose the failed response
                    response.Dispose();
                    
                    // Force authorization refresh
                    await _mainClient.ForceAuthorizeAsync(cancellationToken);
                    
                    // Retry the request
                    continue;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && retried)
                {
                    // Already retried once, this is a real authentication failure
                    string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Authentication failed after retry. Status: {response.StatusCode}, Content: {errorContent}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Failed to get embeddings. Status: {response.StatusCode}, Content: {errorContent}");
                }

                // Success, break out of retry loop
                break;
            }

            string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            EmbeddingResponse embeddingResponse = EmbeddingResponse.FromJson(responseJson);

            return embeddingResponse;
        }
    }
}