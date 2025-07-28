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
    }
}