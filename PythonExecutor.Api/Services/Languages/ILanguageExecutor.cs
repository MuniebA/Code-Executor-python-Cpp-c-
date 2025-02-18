using PythonExecutor.Api.Models;

namespace PythonExecutor.Api.Services.Languages
{
    public interface ILanguageExecutor
    {
        string Language { get; }
        Task<CodeExecutionResult> ExecuteCodeAsync(string code, string inputs, CancellationToken cancellationToken);
        string GetFileExtension();
        string GetDockerImage();
        string[] GetCompilationCommand(string filePath);
        string[] GetExecutionCommand(string filePath);
    }
}