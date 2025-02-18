using System;
using System.Threading;
using System.Threading.Tasks;

namespace PythonExecutor.Tests.Models
{
    public class TestCodeExecutionResult
    {
        public string Output { get; set; } = string.Empty;
        public int ExitCode { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string ModifiedCode { get; set; } = string.Empty;
        public bool RequiresCompilation { get; set; }
    }

    public interface ITestDockerService
    {
        Task<TestCodeExecutionResult> ExecutePythonCodeAsync(
            string code,
            string inputs = "",
            string language = "python",
            CancellationToken cancellationToken = default);
    }
}