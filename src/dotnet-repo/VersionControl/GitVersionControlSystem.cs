using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotNet.Repo.VersionControl
{
    public class GitVersionControlSystem : VersionControlSystem
    {
        private readonly Tool _git;
        private static readonly Lazy<Task<string>> _gitignoreContent = new Lazy<Task<string>>(LoadGitIgnore);
        private readonly ILogger<GitVersionControlSystem> _logger;

        public override bool IsInstalled => _git != null;
        public override string Name => "git";

        public GitVersionControlSystem(ILoggerFactory loggerFactory)
        {
            _git = Tool.Locate("git", loggerFactory);
            _logger = loggerFactory.CreateLogger<GitVersionControlSystem>();
        }

        public override async Task<bool> TryInitializeAsync(string repositoryRoot)
        {
            if (_git == null)
            {
                throw new CommandLineException("Unable to locate 'git' on the system PATH!");
            }

            // Init the repo
            _logger.LogInformation("Creating git repository in '{RepositoryRoot}'", repositoryRoot);
            var result = await _git.Arguments("init")
                .InDirectory(repositoryRoot)
                .ExecuteAsync();
            if (!result.Success)
            {
                return false;
            }

            // Drop the gitignore
            // TODO: Consider getting the latest version of this from https://github.com/github/gitignore
            _logger.LogDebug("Adding .gitignore file");
            var gitignore = Path.Combine(repositoryRoot, ".gitignore");
            using (var writer = new StreamWriter(gitignore))
            {
                await writer.WriteAsync(await _gitignoreContent.Value);
            }

            return true;
        }

        private static async Task<string> LoadGitIgnore()
        {
            using (var stream = typeof(GitVersionControlSystem).Assembly.GetManifestResourceStream("DotNet.Repo.Resources.VisualStudio.gitignore"))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
