using System.Text.Json;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Represents a serializable exception response that can be transmitted over network boundaries.
    /// </summary>
    public class ExceptionResponse
    {
        /// <summary>
        /// Gets or sets the type of the original exception.
        /// </summary>
        public string ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets the message from the original exception.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionResponse"/> class.
        /// </summary>
        /// <param name="exceptionType">The type of the original exception.</param>
        /// <param name="exceptionMessage">The message from the original exception.</param>
        public ExceptionResponse(Type exceptionType, string exceptionMessage)
        {
            ExceptionType = exceptionType.FullName ?? "Exception";
            ExceptionMessage = exceptionMessage;
        }

        /// <summary>
        /// Serializes this exception response to JSON format.
        /// </summary>
        /// <returns>A JSON string representation of this exception response.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, JsonSerializerSettings.CurrentOptions);
        }

        /// <summary>
        /// Deserializes an exception response from JSON format.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An <see cref="ExceptionResponse"/> object. If deserialization fails, returns a fallback response indicating the failure.</returns>
        public static ExceptionResponse FromJson(string json)
        {
            ExceptionResponse? exceptionResponse = JsonSerializer.Deserialize<ExceptionResponse>(json);

            if (exceptionResponse != null)
                return exceptionResponse;

            return new ExceptionResponse(typeof(Exception), $"Could not deserialize original exception response from json: {json}");
        }
    }
}
