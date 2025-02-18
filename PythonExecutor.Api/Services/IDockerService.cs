using PythonExecutor.Api.Models;

namespace PythonExecutor.Api.Services
{
    public interface IDockerService
    {
        Task<CodeExecutionResult> ExecutePythonCodeAsync(
            string code,
            string inputs = "",
            string language = "python",
            CancellationToken cancellationToken = default);
    }
}