namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Request model for chat with tools.
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// Gets or sets the name of the model to use.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the list of messages.
        /// </summary>
        public List<Message> Messages { get; set; }

        /// <summary>
        /// Gets or sets the list of tool definitions.
        /// </summary>
        public IEnumerable<ToolDefinition>? ToolDefinitions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRequest"/> class.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="messages">The list of messages.</param>
        /// <param name="toolDefinitions">The optional list of tool definitions.</param>
        public ChatRequest(string modelName, List<Message> messages, IEnumerable<ToolDefinition>? toolDefinitions = null)
        {
            ModelName = modelName;
            Messages = messages;
            ToolDefinitions = toolDefinitions;
        }
    }
}