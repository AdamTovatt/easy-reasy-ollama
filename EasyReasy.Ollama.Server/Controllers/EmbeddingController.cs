using EasyReasy.Ollama.Common;
using EasyReasy.Ollama.Server.Services.Ollama;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyReasy.Ollama.Server.Controllers
{
    [ApiController]
    [Route("api/embeddings")]
    [Authorize]
    public class EmbeddingController : ControllerBase
    {
        private readonly IOllamaServiceFactory _ollamaServiceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddingController"/> class.
        /// </summary>
        public EmbeddingController(IOllamaServiceFactory ollamaServiceFactory)
        {
            _ollamaServiceFactory = ollamaServiceFactory;
        }

        /// <summary>
        /// Gets embeddings for the given text using the specified model.
        /// </summary>
        /// <param name="request">The request containing model name and text to embed.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The embedding response containing the embedding vector.</returns>
        [HttpPost]
        public async Task<ActionResult<EmbeddingResponse>> GetEmbeddings([FromBody] EmbeddingRequest request, CancellationToken cancellationToken)
        {
            try
            {
                IOllamaEmbeddingService embeddingService = _ollamaServiceFactory.GetEmbeddingService(request.ModelName);
                float[] embeddings = await embeddingService.GetEmbeddingsAsync(request.Text, cancellationToken);
                return Ok(new EmbeddingResponse(embeddings));
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(new ExceptionResponse(argumentException.GetType(), argumentException.Message));
            }
            catch (Exception exception)
            {
                return StatusCode(500, new ExceptionResponse(exception.GetType(), exception.Message));
            }
        }
    }
}