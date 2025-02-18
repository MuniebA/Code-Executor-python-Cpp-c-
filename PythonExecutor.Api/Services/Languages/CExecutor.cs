using PythonExecutor.Api.Models;

namespace PythonExecutor.Api.Services.Languages
{
    public class CLanguageExecutor : ILanguageExecutor
    {
        public string Language => "c";

        public string GetDockerImage() => "gcc:latest";

        public string GetFileExtension() => ".c";

        public string[] GetCompilationCommand(string filePath) =>
            new[] { "gcc", filePath, "-o", Path.ChangeExtension(filePath, null) };

        public string[] GetExecutionCommand(string filePath) =>
            new[] { Path.ChangeExtension(filePath, null) };

        public Task<CodeExecutionResult> ExecuteCodeAsync(string code, string inputs, CancellationToken cancellationToken)
        {
            // Remove any existing main function from user code
            var userCode = code.Replace("int main()", "")
                              .Replace("int main(void)", "")
                              .Trim().Trim('{', '}').Trim();

            // Create input file content with proper escaping
            var escapedInputs = inputs.Replace("\"", "\\\"").Replace("\n", "\\n");

            // Modify code to handle input
            var modifiedCode = @"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

// Buffer for storing input
static char input_buffer[4096];
static const char* current_pos = NULL;

// Custom scanf implementation that reads from our input string
int custom_scanf(const char* format, void* ptr) {
    if (!current_pos || *current_pos == '\0') return EOF;
    
    // Skip whitespace
    while (*current_pos == ' ' || *current_pos == '\n') current_pos++;
    
    if (format[0] == '%') {
        switch(format[1]) {
            case 'd': {
                int* iptr = (int*)ptr;
                *iptr = atoi(current_pos);
                while (*current_pos && *current_pos != '\n') current_pos++;
                if (*current_pos) current_pos++;
                return 1;
            }
            // Add other format specifiers as needed
        }
    }
    return 0;
}

#define scanf custom_scanf

int main(void) {
    // Initialize input
    const char* input_data = """ + escapedInputs + @""";
    current_pos = input_data;

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