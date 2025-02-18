namespace PythonExecutor.Web.Models
{
    public class CodeExecutionRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Inputs { get; set; } = string.Empty;
        public string Language { get; set; } = "python";
    }
}