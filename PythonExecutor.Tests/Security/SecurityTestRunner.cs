using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PythonExecutor.Tests.Services;

namespace PythonExecutor.Tests.Security
{
    public class SecurityTestRunner
    {
        private readonly IDockerServiceTest _dockerService;
        private readonly ILogger<SecurityTestRunner> _logger;

        public SecurityTestRunner(IDockerServiceTest dockerService, ILogger<SecurityTestRunner> logger)
        {
            _dockerService = dockerService;
            _logger = logger;
        }

        public async Task RunAllTests()
        {
            var tests = new Dictionary<string, string>
            {
                { "Memory Test", GetMemoryTest() },
                { "CPU Test", GetCPUTest() },
                { "Infinite Loop Test", GetInfiniteLoopTest() },
                { "File System Test", GetFileSystemTest() },
                { "Network Test", GetNetworkTest() },
                { "Process Test", GetProcessTest() }
            };

            foreach (var test in tests)
            {
                _logger.LogInformation($"Running {test.Key}...");

                try
                {
                    var result = await _dockerService.ExecutePythonCodeAsync(
                        test.Value,
                        string.Empty,
                        "python");

                    _logger.LogInformation($"Test: {test.Key}");
                    _logger.LogInformation($"Success: {result.Success}");
                    _logger.LogInformation($"Output: {result.Output}");
                    _logger.LogInformation($"Error: {result.Error}");
                    _logger.LogInformation($"Exit Code: {result.ExitCode}");
                    _logger.LogInformation("------------------------");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Test {test.Key} failed with exception: {ex.Message}");
                }
            }
        }

        private string GetMemoryTest() => @"data = [1] * 1000000000";
        private string GetCPUTest() => @"def factorial(n):
    if n == 0: return 1
    return n * factorial(n-1)
print(factorial(100000))";
        private string GetInfiniteLoopTest() => @"while True: print('Testing timeout')";
        private string GetFileSystemTest() => @"with open('/etc/passwd', 'r') as f: print(f.read())";
        private string GetNetworkTest() => @"import urllib.request
urllib.request.urlopen('http://google.com')";
        private string GetProcessTest() => @"import subprocess
subprocess.run(['ls'])";
    }
}