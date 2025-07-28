using EasyReasy.Ollama.Common;
using EasyReasy.Ollama.Server.Services.Ollama;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyReasy.Ollama.Server.Controllers
{
    [ApiController]
    [Route("api/ollama")]
    [Authorize]
    public class OllamaController : ControllerBase
    {
        private readonly IOllamaServiceFactory _ollamaServiceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OllamaController"/> class.
        /// </summary>
        public OllamaController(IOllamaServiceFactory ollamaServiceFactory)
        {
            _ollamaServiceFactory = ollamaServiceFactory;
        }

        /// <summary>
        /// Gets a list of all available models.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A list of available model names.</returns>
        [HttpGet("models")]
        public async Task<ActionResult<List<string>>> GetAvailableModels(CancellationToken cancellationToken)
        {
            try
            {
                List<string> models = await _ollamaServiceFactory.GetAvailableModelsAsync(cancellationToken);
                return Ok(models);
            }
            catch (Exception exception)
            {
                return StatusCode(500, new ExceptionResponse(exception.GetType(), exception.Message));
            }
        }

        /// <summary>
        /// Checks if a specific model is available.
        /// </summary>
        /// <param name="modelName">The name of the model to check.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>True if the model is available; otherwise, false.</returns>
        [HttpGet("models/{modelName}/available")]
        public async Task<ActionResult<bool>> IsModelAvailable(string modelName, CancellationToken cancellationToken)
        {
            try
            {
                bool isAvailable = await _ollamaServiceFactory.IsModelAvailableAsync(modelName, cancellationToken);
                return Ok(isAvailable);
            }
            catch (Exception exception)
            {
                return StatusCode(500, new ExceptionResponse(exception.GetType(), exception.Message));
            }
        }
    }
}
