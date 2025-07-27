namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Represents a tool definition that can be used with Ollama function calling.
    /// </summary>
    public class ToolDefinition
    {
        /// <summary>
        /// Gets or sets the description of the function.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameters of the function.
        /// </summary>
        public List<PossibleParameter>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the type of the tool. Typically "function".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolDefinition"/> class.
        /// </summary>
        /// <param name="name">The name of the tool.</param>
        /// <param name="description">The description of the tool.</param>
        /// <param name="possibleParameters">The parameters that can be passed to the tool.</param>
        /// <param name="type">The type of the tool. Defaults to "function".</param>
        public ToolDefinition(string name, string? description, List<PossibleParameter>? possibleParameters, string type = "function")
        {
            Name = name;
            Description = description;
            Parameters = possibleParameters;
            Type = type;
        }
    }
}
