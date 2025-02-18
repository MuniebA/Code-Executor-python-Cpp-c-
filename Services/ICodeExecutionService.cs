using PythonExecutor.Web.Models;

namespace PythonExecutor.Web.Services
{
    public interface ICodeExecutionService
    {
        Task<CodeExecutionResult> ExecuteCodeAsync(CodeExecutionRequest request);
    }
}