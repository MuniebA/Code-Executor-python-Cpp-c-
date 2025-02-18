using System.Net.Http.Json;
using PythonExecutor.Web.Models;

using System.Net.Http.Json;

namespace PythonExecutor.Web.Services
{
    public class CodeExecutionService : ICodeExecutionService
    {
        private readonly HttpClient _httpClient;

        public CodeExecutionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CodeExecutionResult> ExecuteCodeAsync(CodeExecutionRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/CodeExecution/execute", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CodeExecutionResult>();
        }
    }
}