using Microsoft.AspNetCore.Mvc;

namespace EasyReasy.Ollama.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OllamaController : ControllerBase
    {
        private readonly ILogger<OllamaController> _logger;

        public OllamaController(ILogger<OllamaController> logger)
        {
            _logger = logger;
        }
    }
}
