using System;
using System.IO;
using System.Threading.Tasks;
using DotNet.Repo.Resources;
using Microsoft.Extensions.Logging;

namespace DotNet.Repo.Build
{
    public class BuildSystem
    {
        private static readonly Lazy<Task<string>> _directoryBuildPropsContent = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("Directory.Build.props.template"));
        private static readonly Lazy<Task<string>> _directoryBuildTargetsContent = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("Directory.Build.targets.template"));
        private readonly ILogger<BuildSystem> _logger;

        public BuildSystem(ILogger<BuildSystem> logger)
        {
            _logger = logger;
        }

        public async Task InitializeAsync(string rootDirectory)
        {
            _logger.LogInformation("Installing build scripts...");

            // Drop the Directory.Build.props and Directory.Build.targets files to reference the build modules
            var propsFile = Path.Combine(rootDirectory, "Directory.Build.props");
            var targetsFile = Path.Combine(rootDirectory, "Directory.Build.targets");

            await File.WriteAllTextAsync(propsFile, await _directoryBuildPropsContent.Value);
            await File.WriteAllTextAsync(targetsFile, await _directoryBuildTargetsContent.Value);
        }
    }
}
