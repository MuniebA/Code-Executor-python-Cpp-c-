using PythonExecutor.Api.Models;

namespace PythonExecutor.Api.Services.Languages
{
    public class CppExecutor : ILanguageExecutor
    {
        public string Language => "cpp";

        public string GetDockerImage() => "gcc:latest";

        public string GetFileExtension() => ".cpp";

        public string[] GetCompilationCommand(string filePath) =>
            new[] { "g++", filePath, "-o", Path.ChangeExtension(filePath, null) };

        public string[] GetExecutionCommand(string filePath) =>
            new[] { Path.ChangeExtension(filePath, null) };

        public Task<CodeExecutionResult> ExecuteCodeAsync(string code, string inputs, CancellationToken cancellationToken)
        {
            // Remove any existing main function and namespace declarations from user code
            var userCode = code.Replace("int main()", "")
                              .Replace("int main(void)", "")
                              .Replace("using namespace std;", "")
                              .Trim().Trim('{', '}').Trim();

            // Create input file content with proper escaping
            var escapedInputs = inputs.Replace("\"", "\\\"").Replace("\n", "\\n");

            // Modify code to handle input
            var modifiedCode = @"
#include <iostream>
#include <string>
#include <sstream>

using namespace std;  // Add this to allow unqualified names

int main() {
    // Set up input from string
    string input_data = """ + escapedInputs + @""";
    istringstream input_stream(input_data);
    cin.rdbuf(input_stream.rdbuf());

    // User code starts here
    " + userCode + @"

    return 0;
}";

            return Task.FromResult(new CodeExecutionResult
            {
                ModifiedCode = modifiedCode,
                RequiresCompilation = true,
                Success = true,
                ExecutionTime = DateTime.UtcNow
            });
        }
    }
}