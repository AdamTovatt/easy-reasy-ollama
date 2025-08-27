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

            // First, try to find the start of the JSON object
            int startIndex = json.IndexOf('{');
            if (startIndex == -1)
            {
                return json;
            }

            // Extract the JSON part starting from the first '{'
            string jsonPart = json.Substring(startIndex);

            // Try to parse with Utf8JsonReader first (for complete JSON)
            try
            {
                return CleanCompleteJson(jsonPart);
            }
            catch (JsonException)
            {
                // If parsing fails, try to fix incomplete JSON
                return FixIncompleteJson(jsonPart);
            }
        }

        private string CleanCompleteJson(string json)
        {
            StringBuilder result = new StringBuilder();

            ReadOnlySpan<byte> jsonBytes = Encoding.UTF8.GetBytes(json);
            Utf8JsonReader reader = new Utf8JsonReader(jsonBytes);
            int? outerElementDepth = null;
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

                    if (outerElementDepth != null && reader.CurrentDepth == outerElementDepth)
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

                    if (outerElementDepth == null && reader.ValueTextEquals(_outerElementName))
                    {
                        outerElementDepth = reader.CurrentDepth;
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

        private string FixIncompleteJson(string json)
        {
            // Simple approach: just add missing quotes and braces
            string result = json;

            // If the string ends with a quote, it's incomplete
            if (result.EndsWith("\""))
            {
                // Find the last unescaped quote
                int lastQuoteIndex = -1;
                bool escaped = false;
                for (int i = result.Length - 1; i >= 0; i--)
                {
                    if (result[i] == '\\')
                    {
                        escaped = !escaped;
                    }
                    else if (result[i] == '"' && !escaped)
                    {
                        lastQuoteIndex = i;
                        break;
                    }
                    else
                    {
                        escaped = false;
                    }
                }

                // If we found a quote and it's not at the end, we need to close the string
                if (lastQuoteIndex != -1 && lastQuoteIndex < result.Length - 1)
                {
                    // The string is already properly closed
                }
                else
                {
                    // Add closing quote
                    result += "\"";
                }
            }
            else if (!result.EndsWith("\"") && !result.EndsWith("}") && !result.EndsWith("]") && !result.EndsWith(","))
            {
                // If it doesn't end with a quote, brace, bracket, or comma, it might be an incomplete string
                // Find the last property name and see if it's missing a value
                int lastColonIndex = result.LastIndexOf(':');
                if (lastColonIndex != -1)
                {
                    string afterColon = result.Substring(lastColonIndex + 1).Trim();
                    if (afterColon.StartsWith("\"") && !afterColon.EndsWith("\""))
                    {
                        // Incomplete string value
                        result += "\"";
                    }
                }
            }

            // Check if we have an incomplete string that ends with a brace
            if (result.EndsWith("}") && !result.EndsWith("\"}"))
            {
                // Find the last quote before the closing brace
                int lastQuoteIndex = result.LastIndexOf('"');
                if (lastQuoteIndex != -1)
                {
                    // Check if the last quote is part of a property name or value
                    int lastColonIndex = result.LastIndexOf(':');
                    if (lastColonIndex != -1 && lastQuoteIndex > lastColonIndex)
                    {
                        // The last quote is part of a value, so we need to close it
                        result = result.Insert(result.Length - 1, "\"");
                    }
                }
            }

            // Count braces and add missing ones
            int openBraces = 0;
            int closeBraces = 0;
            bool inString = false;
            bool escaped2 = false;

            for (int i = 0; i < result.Length; i++)
            {
                char c = result[i];

                if (escaped2)
                {
                    escaped2 = false;
                    continue;
                }

                if (c == '\\')
                {
                    escaped2 = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (!inString)
                {
                    if (c == '{')
                        openBraces++;
                    else if (c == '}')
                        closeBraces++;
                }
            }

            // Add missing closing braces
            for (int i = 0; i < openBraces - closeBraces; i++)
            {
                result += "}";
            }

            return ReplaceFieldNames(result);
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
