using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using PythonExecutor.Tests.Services;

namespace PythonExecutor.Tests.Security
{
    public class SecurityTests
    {
        private readonly Mock<IDockerServiceTest> _mockDockerService;
        private readonly Mock<ILogger<SecurityTestRunner>> _mockLogger;
        private readonly SecurityTestRunner _testRunner;

        public SecurityTests()
        {
            _mockDockerService = new Mock<IDockerServiceTest>();
            _mockLogger = new Mock<ILogger<SecurityTestRunner>>();
            _testRunner = new SecurityTestRunner(_mockDockerService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RunAllSecurityTests()
        {
            // Configure mock
            _mockDockerService
                .Setup(x => x.ExecutePythonCodeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CodeExecutionTestResult
                {
                    Success = true,
                    Output = "Test output",
                    ExecutionTime = DateTime.UtcNow
                });

            await _testRunner.RunAllTests();

            // Verify Docker service was called for each test
            _mockDockerService.Verify(
                x => x.ExecutePythonCodeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.AtLeast(6));
        }
    }
}