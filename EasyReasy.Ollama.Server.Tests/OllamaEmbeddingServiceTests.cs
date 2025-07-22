using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyReasy.EnvironmentVariables;
using EasyReasy.Ollama.Server.Services.Ollama;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyReasy.Ollama.Server.Tests
{
    [TestClass]
    public class OllamaEmbeddingServiceTests
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
        public async Task EmbeddingService_ReturnsEmbeddings()
        {
            using OllamaEmbeddingService service = OllamaEmbeddingService.Create(_url, _model);
            float[] embedding = await service.GetEmbeddingsAsync("Hello, world!", CancellationToken.None);
            Assert.IsNotNull(embedding);
            Assert.IsTrue(embedding.Length > 0);
        }
    }
} 