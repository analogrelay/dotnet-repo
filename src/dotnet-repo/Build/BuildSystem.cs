using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotNet.Repo.Build
{
    public class BuildSystem
    {
        private static readonly Lazy<Task<string>> _versionPropsContent = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("version.props"));
        private static readonly Lazy<Task<string>> _directoryBuildPropsContent = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("Directory.Build.props.template"));
        private static readonly Lazy<Task<string>> _directoryBuildTargetsContent = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("Directory.Build.targets.template"));
        private static readonly Lazy<Task<string>> _commonPropsContent = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("common.props"));
        private static readonly Lazy<Task<string>> _commonTargetsContent = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("common.targets"));
        private readonly ILogger<BuildSystem> _logger;

        public BuildSystem(ILogger<BuildSystem> logger)
        {
            _logger = logger;
        }

        public async Task InitializeAsync(string rootDirectory)
        {
            _logger.LogInformation("Installing build scripts...");

            var buildDirectory = Path.Combine(rootDirectory, "build");
            if(!Directory.Exists(buildDirectory))
            {
                Directory.CreateDirectory(buildDirectory);
            }

            // Drop the Directory.Build.props and Directory.Build.targets files to reference the build modules
            var propsFile = Path.Combine(rootDirectory, "Directory.Build.props");
            var targetsFile = Path.Combine(rootDirectory, "Directory.Build.targets");
            var commonPropsFile = Path.Combine(buildDirectory, "common.props");
            var commonTargetsFile = Path.Combine(buildDirectory, "common.targets");
            var versionPropsFile = Path.Combine(rootDirectory, "version.props");

            await File.WriteAllTextAsync(propsFile, await _directoryBuildPropsContent.Value);
            await File.WriteAllTextAsync(targetsFile, await _directoryBuildTargetsContent.Value);
            await File.WriteAllTextAsync(commonPropsFile, await _commonPropsContent.Value);
            await File.WriteAllTextAsync(commonTargetsFile, await _commonTargetsContent.Value);
            await File.WriteAllTextAsync(versionPropsFile, await _versionPropsContent.Value);
        }
    }
}
