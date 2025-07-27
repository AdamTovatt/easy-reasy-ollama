namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Represents a parameter of a function.
    /// </summary>
    public class PossibleParameter
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the description of the parameter.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether the parameter is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleParameter"/> class.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="description">The description of the parameter.</param>
        /// <param name="required">Whether the parameter is required.</param>
        public PossibleParameter(string parameterName, string type, string? description, bool required)
        {
            ParameterName = parameterName;
            Type = type;
            Description = description;
            Required = required;
        }
    }
}