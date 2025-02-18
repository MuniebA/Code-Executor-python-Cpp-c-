using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using PythonExecutor.Tests.Models;

namespace PythonExecutor.Tests.Performance
{
    public class PerformanceTests
    {
        private readonly Mock<ITestDockerService> _mockDockerService;
        private readonly Mock<ILogger<PerformanceTestRunner>> _mockLogger;
        private readonly PerformanceTestRunner _testRunner;

        public PerformanceTests()
        {
            _mockDockerService = new Mock<ITestDockerService>();
            _mockLogger = new Mock<ILogger<PerformanceTestRunner>>();
            _testRunner = new PerformanceTestRunner(_mockDockerService.Object, _mockLogger.Object);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        public async Task TestConcurrentExecution(int numberOfRequests)
        {
            // Configure mock
            _mockDockerService
                .Setup(x => x.ExecutePythonCodeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestCodeExecutionResult
                {
                    Success = true,
                    Output = "Test output",
                    ExecutionTime = DateTime.UtcNow
                });

            await _testRunner.RunConcurrencyTest(numberOfRequests);

            // Verify Docker service was called the expected number of times
            _mockDockerService.Verify(
                x => x.ExecutePythonCodeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(numberOfRequests));
        }
    }
}