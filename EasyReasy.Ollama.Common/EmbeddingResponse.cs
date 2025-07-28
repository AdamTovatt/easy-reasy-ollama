using System.Text.Json;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Response model for embeddings.
    /// </summary>
    public class EmbeddingResponse
    {
        /// <summary>
        /// Gets or sets the embedding vector.
        /// </summary>
        public float[] Embeddings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddingResponse"/> class.
        /// </summary>
        /// <param name="embeddings">The embedding vector.</param>
        public EmbeddingResponse(float[] embeddings)
        {
            Embeddings = embeddings;
        }

        /// <summary>
        /// Serializes this <see cref="EmbeddingResponse"/> instance to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of this <see cref="EmbeddingResponse"/> instance.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, JsonSerializerSettings.CurrentOptions);
        }

        /// <summary>
        /// Returns a JSON string representation of this <see cref="EmbeddingResponse"/> instance.
        /// </summary>
        /// <returns>A JSON string representation of this <see cref="EmbeddingResponse"/> instance.</returns>
        public override string ToString()
        {
            return ToJson();
        }

        /// <summary>
        /// Creates an <see cref="EmbeddingResponse"/> instance from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An <see cref="EmbeddingResponse"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the JSON cannot be deserialized into an <see cref="EmbeddingResponse"/>.</exception>
        public static EmbeddingResponse FromJson(string json)
        {
            try
            {
                EmbeddingResponse? result = JsonSerializer.Deserialize<EmbeddingResponse>(json, JsonSerializerSettings.CurrentOptions);

                if (result == null)
                {
                    throw new ArgumentException($"Failed to deserialize {nameof(EmbeddingResponse)} from json: {json}");
                }

                return result;
            }
            catch (JsonException jsonException)
            {
                throw new ArgumentException($"Failed to deserialize {nameof(EmbeddingResponse)} from json: {json}", jsonException);
            }
        }
    }
}