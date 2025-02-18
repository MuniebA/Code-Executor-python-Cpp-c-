namespace PythonExecutor.Web.Models
{
    public class CodeExecutionResult
    {
        public string Output { get; set; } = string.Empty;
        public int ExitCode { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string ModifiedCode { get; set; } = string.Empty;
        public bool RequiresCompilation { get; set; }
    }
}