using System.Reflection;
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

        /// <summary>
        /// Attempts to recreate the original exception using reflection.
        /// Only works if the exception type exists in the current assembly.
        /// </summary>
        /// <returns>The recreated exception, or a generic Exception if recreation fails.</returns>
        public Exception RecreateException()
        {
            try
            {
                // Try to find the exception type in the current assembly
                Type? exceptionType = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .FirstOrDefault(t => t.FullName == ExceptionType && typeof(Exception).IsAssignableFrom(t));

                if (exceptionType != null)
                {
                    // Try to create the exception using the message constructor
                    Exception? exception = Activator.CreateInstance(exceptionType, ExceptionMessage) as Exception;
                    if (exception != null)
                    {
                        return exception;
                    }
                }
            }
            catch
            {
                // Fall through to generic exception
            }

            // Fallback to generic exception
            return new Exception(ExceptionMessage);
        }
    }
}
