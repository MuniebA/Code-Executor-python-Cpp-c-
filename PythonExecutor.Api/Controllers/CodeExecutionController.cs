using Microsoft.AspNetCore.Mvc;
using PythonExecutor.Api.Models;
using PythonExecutor.Api.Services;

namespace PythonExecutor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodeExecutionController : ControllerBase
    {
        private readonly IDockerService _dockerService;
        private readonly ILogger<CodeExecutionController> _logger;

        public CodeExecutionController(
            IDockerService dockerService,
            ILogger<CodeExecutionController> logger)
        {
            _dockerService = dockerService;
            _logger = logger;
        }

        [HttpPost("execute")]
        public async Task<ActionResult<CodeExecutionResult>> ExecuteCode(
            [FromBody] CodeExecutionRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Code cannot be empty");
            }

            try
            {
                var result = await _dockerService.ExecutePythonCodeAsync(
                    request.Code,
                    request.Inputs,
                    request.Language,
                    cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing code");
                return StatusCode(500, "An error occurred while executing the code");
            }
        }
    }
}