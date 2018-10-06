using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotNet.Repo
{
    public class SolutionManager
    {
        private readonly ToolSet _tools;
        private readonly ILogger<SolutionManager> _logger;

        public SolutionManager(ToolSet tools, ILogger<SolutionManager> logger)
        {
            _tools = tools;
            _logger = logger;
        }

        public Task CreateSolutionAsync(string path)
        {
            if (!path.EndsWith(".sln"))
            {
                throw new ArgumentException("Path must end in '.sln'", nameof(path));
            }

            var directory = Path.GetDirectoryName(path);
            var name = Path.GetFileNameWithoutExtension(path);

            return _tools.DotNet.Arguments("new", "sln", "--name", name)
                .InDirectory(directory)
                .ExecuteAsync(throwOnFailure: true);
        }

        public async Task AddNewProjectAsync(string solutionPath, string projectGroup, string projectName, string template)
        {
            var baseDirectory = Path.GetDirectoryName(solutionPath);
            var groupDirectory = Path.Combine(baseDirectory, projectGroup);

            if(!Directory.Exists(groupDirectory))
            {
                _logger.LogDebug("Creating directory '{directory}'", groupDirectory);
                Directory.CreateDirectory(groupDirectory);
            }

            // Expand the template
            _logger.LogInformation("Creating project '{projectGroup}/{projectName}' using template '{template}'", projectGroup, projectName, template);
            await _tools.DotNet.Arguments("new", template, "--name", projectName)
                .InDirectory(groupDirectory)
                .ExecuteAsync();

            // Add to the solution
            // TODO: VB?
            var proj = Path.Combine(groupDirectory, projectName, $"{projectName}.csproj");
            if(File.Exists(proj))
            {
                await AddProjectAsync(solutionPath, proj);
            }
        }

        private Task AddProjectAsync(string solutionPath, string project)
        {
            _logger.LogInformation("Adding '{project}' to solution '{solutionPath}'", project, solutionPath);
            return _tools.DotNet.Arguments("sln", solutionPath, "add", project)
                .ExecuteAsync(throwOnFailure: true);
        }
    }
}
