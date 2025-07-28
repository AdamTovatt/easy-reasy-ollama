namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Exception thrown when a model is not allowed for the current request or configuration.
    /// </summary>
    public class ModelNotAllowedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelNotAllowedException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ModelNotAllowedException(string? message) : base(message) { }
    }
}
