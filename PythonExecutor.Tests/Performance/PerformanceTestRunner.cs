using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PythonExecutor.Tests.Models;

namespace PythonExecutor.Tests.Performance
{
    public class PerformanceTestRunner
    {
        private readonly ITestDockerService _dockerService;
        private readonly ILogger<PerformanceTestRunner> _logger;

        public PerformanceTestRunner(ITestDockerService dockerService, ILogger<PerformanceTestRunner> logger)
        {
            _dockerService = dockerService;
            _logger = logger;
        }

        public async Task RunConcurrencyTest(int numberOfConcurrentRequests)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var tasks = new List<Task>();
            for (int i = 0; i < numberOfConcurrentRequests; i++)
            {
                tasks.Add(RunSingleTest(i));
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            _logger.LogInformation($"Total time for {numberOfConcurrentRequests} concurrent requests: {stopwatch.ElapsedMilliseconds}ms");
            _logger.LogInformation($"Average time per request: {stopwatch.ElapsedMilliseconds / numberOfConcurrentRequests}ms");
        }

        private async Task RunSingleTest(int testNumber)
        {
            // Using raw string to avoid interpolation conflicts with Python f-strings
            var code = @"
test_number = " + testNumber + @"
print(f'Test {test_number}')
sum_result = 0
for i in range(1000):
    sum_result += i
print(f'Result: {sum_result}')
";

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await _dockerService.ExecutePythonCodeAsync(
                    code,
                    string.Empty,
                    "python");

                stopwatch.Stop();

                _logger.LogInformation($"Test {testNumber}:");
                _logger.LogInformation($"Execution time: {stopwatch.ElapsedMilliseconds}ms");
                _logger.LogInformation($"Success: {result.Success}");
                _logger.LogInformation("------------------------");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Test {testNumber} failed: {ex.Message}");
            }
        }

        public async Task RunLanguageComparisonTest()
        {
            var tests = new Dictionary<string, (string code, string language)>
            {
                { "Python", (@"
sum_value = 0
for i in range(1000):
    sum_value += i
print(sum_value)", "python") },

                { "C", (@"
#include <stdio.h>
int main() {
    long sum = 0;
    for(int i = 0; i < 1000; i++) sum += i;
    printf(""%ld\n"", sum);
    return 0;
}", "c") },

                { "C++", (@"
#include <iostream>
int main() {
    long sum = 0;
    for(int i = 0; i < 1000; i++) sum += i;
    std::cout << sum << std::endl;
    return 0;
}", "cpp") }
            };

            foreach (var test in tests)
            {
                _logger.LogInformation($"Running {test.Key} test...");

                try
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var result = await _dockerService.ExecutePythonCodeAsync(
                        test.Value.code,
                        string.Empty,
                        test.Value.language);

                    stopwatch.Stop();

                    _logger.LogInformation($"Language: {test.Key}");
                    _logger.LogInformation($"Execution time: {stopwatch.ElapsedMilliseconds}ms");
                    _logger.LogInformation($"Output: {result.Output}");
                    _logger.LogInformation("------------------------");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Test {test.Key} failed: {ex.Message}");
                }
            }
        }
    }
}