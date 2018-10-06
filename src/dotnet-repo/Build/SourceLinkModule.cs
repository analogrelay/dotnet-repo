using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotNet.Repo.Build
{
    public class SourceLinkModule
    {
        private static readonly Lazy<Task<string>> _moduleProps = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("Modules", "SourceLink", "module.props"));
        private readonly ILogger<SourceLinkModule> _logger;

        public SourceLinkModule(ILogger<SourceLinkModule> logger)
        {
            _logger = logger;
        }


        public async Task InstallAsync(string rootDirectory, string moduleDirectory)
        {
            _logger.LogInformation("Installing SourceLink build module...");
            if (!Directory.Exists(moduleDirectory))
            {
                Directory.CreateDirectory(moduleDirectory);
            }

            // Drop the build module
            var propsFile = Path.Combine(moduleDirectory, "module.props");

            var content = await _moduleProps.Value;

            // Replace the template variable. For now, we hard-code GitHub.
            // TODO: Replace with some kind of centralized dependency management?
            content = content
                .Replace("[[SOURCELINK_PACKAGE_ID]]", "Microsoft.SourceLink.GitHub")
                .Replace("[[SOURCELINK_PACKAGE_VERSION]]", "1.0.0-beta-63127-02");

            _logger.LogDebug("Installing SourceLink");
            await File.WriteAllTextAsync(propsFile, content);
        }
    }
}
