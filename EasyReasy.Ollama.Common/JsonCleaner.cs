using System.Text;
using System.Text.Json;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Helper class for cleaning and normalizing JSON strings.
    /// </summary>
    public class JsonCleaner
    {
        private readonly string _outerElementName;
        private readonly string[] _fieldNamesToReplace;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonCleaner"/> class.
        /// </summary>
        /// <param name="outerElementName">The name of the outer JSON element to look for.</param>
        /// <param name="fieldNamesToReplace">Array of field names to replace (e.g., "parameters" -> "arguments").</param>
        public JsonCleaner(string outerElementName, params string[] fieldNamesToReplace)
        {
            _outerElementName = outerElementName ?? throw new ArgumentNullException(nameof(outerElementName));
            _fieldNamesToReplace = fieldNamesToReplace ?? throw new ArgumentNullException(nameof(fieldNamesToReplace));
        }

        /// <summary>
        /// Cleans a JSON string by finding the complete JSON object and removing any trailing text.
        /// </summary>
        /// <param name="json">The potentially dirty JSON string.</param>
        /// <returns>A cleaned JSON string containing only the complete JSON object.</returns>
        public string CleanJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return json;
            }

            StringBuilder result = new StringBuilder();

            ReadOnlySpan<byte> jsonBytes = Encoding.UTF8.GetBytes(json);
            Utf8JsonReader reader = new Utf8JsonReader(jsonBytes);
            int? _outerElementDepth = null;
            bool lastTokenAddedComma = false;

            while (reader.Read())
            {
                // Add current token to result here
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    result.Append("{");
                    lastTokenAddedComma = false;
                    continue;
                }
                else if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (lastTokenAddedComma)
                        result.Length--;

                    result.Append("}");
                    lastTokenAddedComma = false;

                    if (_outerElementDepth != null && reader.CurrentDepth == _outerElementDepth)
                        break;
                }
                else if (reader.TokenType == JsonTokenType.Null)
                {
                    result.Append("null,");
                    lastTokenAddedComma = true;
                }
                else if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    result.Append("\"");
                    result.Append(reader.GetString());
                    result.Append("\":");
                    lastTokenAddedComma = false;

                    if (_outerElementDepth == null && reader.ValueTextEquals(_outerElementName))
                    {
                        _outerElementDepth = reader.CurrentDepth;
                        continue;
                    }
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    result.Append("\"");
                    string? value = reader.GetString();

                    if (value != null && reader.ValueIsEscaped)
                        value = value.Replace("\"", "\\\"");

                    result.Append(value);
                    result.Append("\",");
                    lastTokenAddedComma = true;
                }
                else
                {
                    string value = Encoding.UTF8.GetString(reader.ValueSpan);
                    result.Append(value);
                    lastTokenAddedComma = false;
                }
            }

            result.Append("}");

            return ReplaceFieldNames(result.ToString());
        }

        private string ReplaceFieldNames(string json)
        {
            // Replace field names as specified in the constructor
            if (_fieldNamesToReplace.Length % 2 == 0 && _fieldNamesToReplace.Length > 0)
            {
                for (int i = 0; i < _fieldNamesToReplace.Length; i += 2)
                {
                    string oldName = _fieldNamesToReplace[i];
                    string newName = _fieldNamesToReplace[i + 1];

                    json = json.Replace(oldName, newName);
                }
            }

            return json;
        }
    }
}
