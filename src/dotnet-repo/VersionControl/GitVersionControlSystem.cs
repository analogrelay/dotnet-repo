using System;
using System.IO;
using System.Threading.Tasks;

namespace DotNet.Repo.VersionControl
{
    public class GitVersionControlSystem : VersionControlSystem
    {
        public static readonly VersionControlSystem Instance = new GitVersionControlSystem();

        private readonly Tool _git = Tool.Locate("git");
        private static readonly Lazy<Task<string>> _gitignoreContent = new Lazy<Task<string>>(LoadGitIgnore);

        public override bool IsInstalled => _git != null;
        public override string Name => "git";

        private GitVersionControlSystem()
        {
        }

        public override async Task<bool> TryInitializeAsync(string repositoryRoot)
        {
            if (_git == null)
            {
                throw new CommandLineException("Unable to locate 'git' on the system PATH!");
            }

            // Init the repo
            var result = await _git.Arguments("init")
                .InDirectory(repositoryRoot)
                .ExecuteAsync();
            if (!result.Success)
            {
                return false;
            }

            // Drop the gitignore
            // TODO: Consider getting the latest version of this from https://github.com/github/gitignore
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
