using PythonExecutor.Api.Models;

namespace PythonExecutor.Api.Services.Languages
{
    public class PythonLanguageExecutor : ILanguageExecutor
    {
        public string Language => "python";

        public string GetDockerImage() => "python:3.9-slim";

        public string GetFileExtension() => ".py";

        public string[] GetCompilationCommand(string filePath) => Array.Empty<string>();

        public string[] GetExecutionCommand(string filePath) => new[] { "python", "-u", filePath };

        public Task<CodeExecutionResult> ExecuteCodeAsync(string code, string inputs, CancellationToken cancellationToken)
        {
            // Modify code to handle input
            var modifiedCode = @"
import builtins
import sys

class InputManager:
    def __init__(self, inputs):
        self.inputs = inputs.split('\n')
        self.current_index = 0
    
    def get_input(self, prompt=''):
        if self.current_index >= len(self.inputs):
            print('No more inputs available')
            return ''
        
        value = self.inputs[self.current_index]
        self.current_index += 1
        print(prompt + value)
        return value

input_manager = InputManager(''' " + inputs.Replace("'", "\\'") + @" ''')
builtins.input = input_manager.get_input

" + code;

            return Task.FromResult(new CodeExecutionResult
            {
                ModifiedCode = modifiedCode,
                RequiresCompilation = false,
                Success = true,
                ExecutionTime = DateTime.UtcNow
            });
        }
    }
}