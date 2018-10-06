using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Repo.Resources;
using Microsoft.Extensions.Logging;

namespace DotNet.Repo.VersionControl
{
    public class GitVersionControlSystem : VersionControlSystem
    {
        private readonly Tool _git;
        private static readonly Lazy<Task<string>> _gitignoreContent = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("VisualStudio.gitignore"));
        private readonly ILogger<GitVersionControlSystem> _logger;
        private readonly ToolSet _tools;

        public override bool IsInstalled => _git != null;
        public override string Name => "git";

        public GitVersionControlSystem(ToolSet tools, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GitVersionControlSystem>();
            _tools = tools;
        }

        public override async Task InitializeAsync(string repositoryRoot, CancellationToken cancellationToken = default)
        {
            // Init the repo
            _logger.LogInformation("Creating git repository in '{RepositoryRoot}'", repositoryRoot);
            var result = await _tools.Git.Arguments("init")
                .InDirectory(repositoryRoot)
                .ExecuteAsync(throwOnFailure: true);

            // Drop the gitignore
            // TODO: Consider getting the latest version of this from https://github.com/github/gitignore
            _logger.LogDebug("Adding .gitignore file");
            var gitignore = Path.Combine(repositoryRoot, ".gitignore");
            using (var writer = new StreamWriter(gitignore))
            {
                await writer.WriteAsync(await _gitignoreContent.Value);
            }
        }

        public override async Task CommitAsync(string repositoryRoot, string message, bool addAll = true, CancellationToken cancellationToken = default)
        {
            // Write the message to a file, to avoid newline formatting issues and command line length restrictions
            var file = Path.GetTempFileName();
            try
            {
                _logger.LogTrace("Writing commit message to '{file}'");
                await File.WriteAllTextAsync(file, message, cancellationToken);

                if (addAll)
                {
                    // Do a 'git add -A' to catch all the new files
                    _logger.LogTrace("Adding all files to pending commit...");
                    await _tools.Git.Arguments("add", "-A")
                        .ExecuteAsync(throwOnFailure: true);
                }

                _logger.LogTrace("Committing...");
                await _tools.Git.Arguments("commit", "-F", file)
                    .ExecuteAsync(throwOnFailure: true);
                _logger.LogTrace("Committed.");
            }
            finally
            {
                if (File.Exists(file))
                {
                    _logger.LogTrace("Deleting temporary file: '{file}'");
                    File.Delete(file);
                }
                else
                {
                    _logger.LogTrace("Temporary file '{file}' does not exist");
                }
            }
        }
    }
}
