using System.Text.Json;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Request model for embeddings.
    /// </summary>
    public class EmbeddingRequest
    {
        /// <summary>
        /// Gets or sets the name of the model to use.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the text to embed.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddingRequest"/> class.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="text">The text to embed.</param>
        public EmbeddingRequest(string modelName, string text)
        {
            ModelName = modelName;
            Text = text;
        }

        /// <summary>
        /// Serializes this <see cref="EmbeddingRequest"/> instance to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of this <see cref="EmbeddingRequest"/> instance.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, JsonSerializerSettings.CurrentOptions);
        }

        /// <summary>
        /// Returns a JSON string representation of this <see cref="EmbeddingRequest"/> instance.
        /// </summary>
        /// <returns>A JSON string representation of this <see cref="EmbeddingRequest"/> instance.</returns>
        public override string ToString()
        {
            return ToJson();
        }

        /// <summary>
        /// Creates an <see cref="EmbeddingRequest"/> instance from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An <see cref="EmbeddingRequest"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the JSON cannot be deserialized into an <see cref="EmbeddingRequest"/>.</exception>
        public static EmbeddingRequest FromJson(string json)
        {
            try
            {
                EmbeddingRequest? result = JsonSerializer.Deserialize<EmbeddingRequest>(json, JsonSerializerSettings.CurrentOptions);

                if (result == null)
                {
                    throw new ArgumentException($"Failed to deserialize {nameof(EmbeddingRequest)} from json: {json}");
                }

                return result;
            }
            catch (JsonException jsonException)
            {
                throw new ArgumentException($"Failed to deserialize {nameof(EmbeddingRequest)} from json: {json}", jsonException);
            }
        }
    }
}