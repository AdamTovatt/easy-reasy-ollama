using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Server.Services.Ollama;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyReasy.Ollama.Server.Tests
{
    [TestClass]
    public class OllamaChatServiceTests
    {
        private string _url;
        private string _model;

        [TestInitialize]
        public void Initialize()
        {
            _url = EnvironmentVariableLoader.Get(OllamaIntegrationEnvironmentVariables.OllamaUrl);
            _model = EnvironmentVariableLoader.Get(OllamaIntegrationEnvironmentVariables.OllamaModelName);
        }

        [TestMethod]
        public async Task ChatService_ReturnsResponse()
        {
            using OllamaChatService service = OllamaChatService.Create(_url, _model);
            IAsyncEnumerable<string> response = service.GetResponseAsync("Hello, world!", CancellationToken.None);
            await foreach (string message in response)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(message));
                break;
            }
        }
    }
} 