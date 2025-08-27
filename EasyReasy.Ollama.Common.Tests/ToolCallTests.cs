namespace EasyReasy.Ollama.Common.Tests
{
    [TestClass]
    public class ToolCallTests
    {
        private static readonly JsonCleaner _jsonCleaner = new JsonCleaner("function", "parameters", "arguments");

        [TestMethod]
        public void FromJson_WithCleanJson_ShouldDeserializeSuccessfully()
        {
            // Arrange
            string cleanJson = @"{""function"":{""index"":null,""name"":""Get current weather"",""arguments"":{""city"":""London""}}}";

            // Act
            ToolCall result = ToolCall.FromJson(cleanJson);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("Get current weather", result.Function.Name);
            Assert.AreEqual("London", result.Function.Arguments?["city"]?.ToString());
        }

        [TestMethod]
        public void FromJson_WithTrailingText_ShouldCleanAndDeserializeSuccessfully()
        {
            // Arrange
            const string dirtyJson = @"{""function"":{""index"":null,""name"":""Get current weather"",""arguments"":{""city"":""London""}} 

I would call the tool with this function call to get a response. The output of the tool would be used to format an answer to the user's question.}";

            const string cleanJson = @"{""function"":{""index"":null,""name"":""Get current weather"",""arguments"":{""city"":""London""}}}";

            // Act
            string cleanedJson = _jsonCleaner.CleanJson(dirtyJson);
            ToolCall result = ToolCall.FromJson(cleanedJson);

            // Assert
            Assert.AreEqual(cleanJson, cleanedJson, $"The json should be cleaned to result in\n{cleanJson} but it was actually\n{cleanedJson}");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("Get current weather", result.Function.Name);
            Assert.AreEqual("London", result.Function.Arguments?["city"]?.ToString());
        }

        [TestMethod]
        public void FromJson_WithEscapedQuotesInArguments_ShouldHandleCorrectly()
        {
            // Arrange
            const string dirtyJson = @"{""function"":{""index"":null,""name"":""Get weather"",""arguments"":{""description"":""It's a \""beautiful\"" day""}}} Some trailing text";

            const string cleanJson = @"{""function"":{""index"":null,""name"":""Get weather"",""arguments"":{""description"":""It's a \""beautiful\"" day""}}}";

            // Act
            string cleanedJson = _jsonCleaner.CleanJson(dirtyJson);
            ToolCall result = ToolCall.FromJson(cleanedJson);

            // Assert
            Assert.AreEqual(cleanJson, cleanedJson, $"The json should be cleaned to result in\n{cleanJson} but it was actually\n{cleanedJson}");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("Get weather", result.Function.Name);
            Assert.AreEqual("It's a \"beautiful\" day", result.Function.Arguments?["description"]?.ToString());
        }

        [TestMethod]
        public void FromJson_WithNestedObjects_ShouldHandleCorrectly()
        {
            // Arrange
            const string dirtyJson = @"{""function"":{""index"":null,""name"":""Complex function"",""arguments"":{""data"":{""nested"":{""value"":123}}}}} Extra text here";

            const string cleanJson = @"{""function"":{""index"":null,""name"":""Complex function"",""arguments"":{""data"":{""nested"":{""value"":123}}}}}";

            // Act
            string cleanedJson = _jsonCleaner.CleanJson(dirtyJson);
            ToolCall result = ToolCall.FromJson(cleanedJson);

            // Assert
            Assert.AreEqual(cleanJson, cleanedJson, $"The json should be cleaned to result in\n{cleanJson} but it was actually\n{cleanedJson}");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("Complex function", result.Function.Name);
            Assert.IsNotNull(result.Function.Arguments);
        }

        [TestMethod]
        public void FromJson_WithParametersInsteadOfArguments_ShouldConvertCorrectly()
        {
            // Arrange
            const string dirtyJson = @"{""function"":{""index"":null,""name"":""Test function"",""parameters"":{""param1"":""value1""}}}";

            const string cleanJson = @"{""function"":{""index"":null,""name"":""Test function"",""arguments"":{""param1"":""value1""}}}";

            // Act
            string cleanedJson = _jsonCleaner.CleanJson(dirtyJson);
            ToolCall result = ToolCall.FromJson(cleanedJson);

            // Assert
            Assert.AreEqual(cleanJson, cleanedJson, $"The json should be cleaned to result in\n{cleanJson} but it was actually\n{cleanedJson}");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("Test function", result.Function.Name);
            Assert.AreEqual("value1", result.Function.Arguments?["param1"]?.ToString());
        }

        [TestMethod]
        public void FromJson_WithComplexTrailingText_ShouldCleanCorrectly()
        {
            // Arrange
            const string dirtyJson = @"{""function"":{""index"":null,""name"":""Weather API"",""arguments"":{""location"":""New York"",""units"":""celsius""}}} 

This function call will retrieve the current weather information for New York in Celsius. The response will include temperature, humidity, and weather conditions. I'll use this data to provide a comprehensive weather report to the user.";

            const string cleanJson = @"{""function"":{""index"":null,""name"":""Weather API"",""arguments"":{""location"":""New York"",""units"":""celsius""}}}";

            // Act
            string cleanedJson = _jsonCleaner.CleanJson(dirtyJson);
            ToolCall result = ToolCall.FromJson(cleanedJson);

            // Assert
            Assert.AreEqual(cleanJson, cleanedJson, $"The json should be cleaned to result in\n{cleanJson} but it was actually\n{cleanedJson}");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("Weather API", result.Function.Name);
            Assert.AreEqual("New York", result.Function.Arguments?["location"]?.ToString());
            Assert.AreEqual("celsius", result.Function.Arguments?["units"]?.ToString());
        }

        [TestMethod]
        public void FromJson_WithOllamaChatServiceScenario_ShouldCleanCorrectly()
        {
            // Arrange
            // This replicates the exact scenario from OllamaChatService where an extra closing brace is added
            const string dirtyJson = @"{""function"":{""index"":null,""name"":""Get current weather"",""arguments"":{""city"":""London""}}} 

I would call the tool with this function call to get a response. The output of the tool would be used to format an answer to the user's question.";

            const string cleanJson = @"{""function"":{""index"":null,""name"":""Get current weather"",""arguments"":{""city"":""London""}}}";

            // Act
            string cleanedJson = _jsonCleaner.CleanJson(dirtyJson);
            ToolCall result = ToolCall.FromJson(cleanedJson);

            // Assert
            Assert.AreEqual(cleanJson, cleanedJson, $"The json should be cleaned to result in\n{cleanJson} but it was actually\n{cleanedJson}");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("Get current weather", result.Function.Name);
            Assert.AreEqual("London", result.Function.Arguments?["city"]?.ToString());
        }

        [TestMethod]
        public void FromJson_WithParametersAndTrailingText_ShouldCleanAndConvertCorrectly()
        {
            // Arrange
            // This replicates the exact scenario you're seeing with "parameters" and trailing text
            const string dirtyJson = @"{""function"":{""index"":null,""name"":""Get current weather"", ""parameters"": {""city"": ""London""}} 

(I'll wait for a response with the output to format an answer)";

            const string cleanJson = @"{""function"":{""index"":null,""name"":""Get current weather"",""arguments"":{""city"":""London""}}}";

            // Act
            string cleanedJson = _jsonCleaner.CleanJson(dirtyJson);
            ToolCall result = ToolCall.FromJson(cleanedJson);

            // Assert
            Assert.AreEqual(cleanJson, cleanedJson, $"The json should be cleaned to result in\n{cleanJson} but it was actually\n{cleanedJson}");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("Get current weather", result.Function.Name);
            Assert.AreEqual("London", result.Function.Arguments?["city"]?.ToString());
        }

        [TestMethod]
        public void FromJson_WithMultipleClosingBracesInTrailingText_ShouldNotBeConfused()
        {
            // Arrange
            const string dirtyJson = @"{""function"":{""index"":null,""name"":""Test"",""arguments"":{""value"":123}}} }}}}}} More text here";

            const string cleanJson = @"{""function"":{""index"":null,""name"":""Test"",""arguments"":{""value"":123}}}";

            // Act
            string cleanedJson = _jsonCleaner.CleanJson(dirtyJson);
            ToolCall result = ToolCall.FromJson(cleanedJson);

            // Assert
            Assert.AreEqual(cleanJson, cleanedJson, $"The json should be cleaned to result in\n{cleanJson} but it was actually\n{cleanedJson}");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("Test", result.Function.Name);
            Assert.AreEqual(123, ((System.Text.Json.JsonElement)result.Function.Arguments?["value"]!).GetInt32());
        }

        [TestMethod]
        public void FromJson_WithSpecificTrailingTextScenario_ShouldCleanCorrectly()
        {
            // Arrange
            const string dirtyJson = @"{""function"":{""index"":null,""name"":""Get current weather"", ""parameters"": {""city"": ""London""}} 

(I'll assume that the temperature will be returned in both Fahrenheit and Celsius by default, as it is a common practice for many weather APIs)}";

            const string cleanJson = @"{""function"":{""index"":null,""name"":""Get current weather"",""arguments"":{""city"":""London""}}}";

            // Act
            string cleanedJson = _jsonCleaner.CleanJson(dirtyJson);
            ToolCall result = ToolCall.FromJson(cleanedJson);

            // Assert
            Assert.AreEqual(cleanJson, cleanedJson, $"The json should be cleaned to result in\n{cleanJson} but it was actually\n{cleanedJson}");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("Get current weather", result.Function.Name);
            Assert.AreEqual("London", result.Function.Arguments?["city"]?.ToString());
        }

        [TestMethod]
        public void FromJson_WithIncompleteJson_ShouldCleanCorrectly()
        {
            // Arrange
            const string dirtyJson = @"{""function"":{""index"":null,""name"":""GetCurrentWeather}";

            const string cleanJson = @"{""function"":{""index"":null,""name"":""GetCurrentWeather""}}";

            // Act
            string cleanedJson = _jsonCleaner.CleanJson(dirtyJson);
            ToolCall result = ToolCall.FromJson(cleanedJson);

            // Assert
            Assert.AreEqual(cleanJson, cleanedJson, $"The json should be cleaned to result in\n{cleanJson} but it was actually\n{cleanedJson}");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Function);
            Assert.AreEqual("GetCurrentWeather", result.Function.Name);
        }
    }
}