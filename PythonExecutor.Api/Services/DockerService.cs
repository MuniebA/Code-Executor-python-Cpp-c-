using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;
using PythonExecutor.Api.Models;
using PythonExecutor.Api.Services.Languages;

namespace PythonExecutor.Api.Services
{
    public class DockerService : IDockerService
    {
        private readonly IDockerClient _dockerClient;
        private readonly ILogger<DockerService> _logger;
        private readonly IEnumerable<ILanguageExecutor> _languageExecutors;
        private static readonly SemaphoreSlim _containerSemaphore = new SemaphoreSlim(3, 3);

        public DockerService(
            ILogger<DockerService> logger,
            IEnumerable<ILanguageExecutor> languageExecutors)
        {
            _logger = logger;
            _languageExecutors = languageExecutors;
            _dockerClient = new DockerClientConfiguration(
                new Uri("npipe://./pipe/docker_engine"))
                    .CreateClient();
        }

        public async Task<CodeExecutionResult> ExecutePythonCodeAsync(
            string code,
            string inputs = "",
            string language = "python",
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _containerSemaphore.WaitAsync(cancellationToken);

                // Get the appropriate language executor
                var executor = _languageExecutors.FirstOrDefault(e => e.Language == language)
                    ?? throw new ArgumentException($"Unsupported language: {language}");

                // Create temporary directory
                var executionId = Guid.NewGuid().ToString();
                var tempPath = Path.Combine(Path.GetTempPath(), $"code-{executionId}");
                Directory.CreateDirectory(tempPath);

                try
                {
                    // Get modified code and prepare files
                    var prepResult = await executor.ExecuteCodeAsync(code, inputs, cancellationToken);
                    var fileName = $"main{executor.GetFileExtension()}";
                    var codePath = Path.Combine(tempPath, fileName);
                    await File.WriteAllTextAsync(codePath, prepResult.ModifiedCode, cancellationToken);

                    // Ensure Docker image exists
                    await EnsureImageExistsAsync(executor.GetDockerImage(), cancellationToken);

                    // Execute code (with compilation if needed)
                    var result = await ExecuteInContainer(executor, tempPath, fileName, prepResult.RequiresCompilation, cancellationToken);
                    return result;
                }
                finally
                {
                    // Cleanup temporary directory
                    try
                    {
                        if (Directory.Exists(tempPath))
                        {
                            Directory.Delete(tempPath, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error cleaning up temporary directory");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing code");
                return new CodeExecutionResult
                {
                    Success = false,
                    Error = ex.Message,
                    ExecutionTime = DateTime.UtcNow
                };
            }
            finally
            {
                _containerSemaphore.Release();
            }
        }

        private async Task<CodeExecutionResult> ExecuteInContainer(
            ILanguageExecutor executor,
            string tempPath,
            string fileName,
            bool requiresCompilation,
            CancellationToken cancellationToken)
        {
            CreateContainerParameters containerConfig = new CreateContainerParameters
            {
                Image = executor.GetDockerImage(),
                AttachStdout = true,
                AttachStderr = true,
                HostConfig = new HostConfig
                {
                    Binds = new[] { $"{tempPath}:/code" },
                    Memory = 52428800, // 50MB
                    CPUQuota = 50000,
                    CPUPeriod = 100000,
                    NetworkMode = "none"
                }
            };

            // If compilation is needed, compile first
            if (requiresCompilation)
            {
                containerConfig.Cmd = executor.GetCompilationCommand($"/code/{fileName}");
                var compileResult = await RunContainer(containerConfig, cancellationToken);
                if (!compileResult.Success)
                {
                    return new CodeExecutionResult
                    {
                        Success = false,
                        Error = "Compilation error:\n" + compileResult.Output,
                        ExecutionTime = DateTime.UtcNow
                    };
                }
            }

            // Execute the code
            containerConfig.Cmd = requiresCompilation
                ? executor.GetExecutionCommand("/code/main")
                : executor.GetExecutionCommand($"/code/{fileName}");

            return await RunContainer(containerConfig, cancellationToken);
        }

        private async Task<CodeExecutionResult> RunContainer(
            CreateContainerParameters config,
            CancellationToken cancellationToken)
        {
            var container = await _dockerClient.Containers.CreateContainerAsync(config, cancellationToken);

            try
            {
                await _dockerClient.Containers.StartContainerAsync(container.ID, null, cancellationToken);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(10)); // 10 second timeout

                var waitResult = await _dockerClient.Containers.WaitContainerAsync(
                    container.ID,
                    cts.Token);

                var logStream = await _dockerClient.Containers.GetContainerLogsAsync(
                    container.ID,
                    new ContainerLogsParameters
                    {
                        ShowStdout = true,
                        ShowStderr = true,
                        Timestamps = false
                    },
                    cts.Token);

                var output = new StringBuilder();
                using (var reader = new StreamReader(logStream))
                {
                    output.Append(await reader.ReadToEndAsync());
                }

                return new CodeExecutionResult
                {
                    Output = output.ToString(),
                    ExitCode = (int)waitResult.StatusCode,
                    Success = waitResult.StatusCode == 0,
                    ExecutionTime = DateTime.UtcNow
                };
            }
            catch (OperationCanceledException)
            {
                return new CodeExecutionResult
                {
                    Success = false,
                    Error = "Execution timed out after 10 seconds",
                    ExecutionTime = DateTime.UtcNow
                };
            }
            finally
            {
                // Cleanup container
                try
                {
                    await _dockerClient.Containers.RemoveContainerAsync(
                        container.ID,
                        new ContainerRemoveParameters { Force = true },
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error removing container");
                }
            }
        }

        private async Task EnsureImageExistsAsync(string image, CancellationToken cancellationToken)
        {
            try
            {
                await _dockerClient.Images.InspectImageAsync(image, cancellationToken);
            }
            catch (DockerImageNotFoundException)
            {
                await _dockerClient.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = image
                    },
                    null,
                    new Progress<JSONMessage>(),
                    cancellationToken);
            }
        }
    }
}