using System.IO;
using System.Threading.Tasks;
using DotNet.Repo.VersionControl;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace DotNet.Repo
{
    [Command(Name = Name, Description = Description)]
    internal class NewCommand
    {
        public const string Name = "new";
        public const string Description = "Create a new repository.";
        private readonly IConsole _console;
        private readonly ILoggerFactory _loggerFactory;

        [Option("--vcs <VERSION_CONTROL_SYSTEM>", Description = "The version control system to use.")]
        public string VcsType { get; set; }

        [Option("-p|--path <PATH>", Description = "The path in which to create the repository. Defaults to '[current directory]/[name]'.")]
        public string RepositoryRoot { get; set; }

        [Argument(0, "<NAME>", Description = "The name of the repository to create. Will also be used as the solution/project name by default")]
        public string RepositoryName { get; set; }

        public NewCommand(IConsole console, ILoggerFactory loggerFactory)
        {
            _console = console;
            _loggerFactory = loggerFactory;
        }

        public async Task<int> OnExecuteAsync()
        {
            // Locate tools
            var dotnet = Tool.Locate("dotnet");
            if (dotnet == null)
            {
                throw new CommandLineException("Unable to locate 'dotnet' on the system PATH!");
            }

            // Identify VCS system
            var vcs = string.IsNullOrEmpty(VcsType) ? GitVersionControlSystem.Instance : VersionControlSystem.GetVersionControlSystem(VcsType);
            if (vcs == null)
            {
                throw new CommandLineException($"Unknown version control system: {VcsType}");
            }

            if (!vcs.IsInstalled)
            {
                _console.Error.WriteLine($"warning: version control system '{VcsType}' is not installed.");
                vcs = VersionControlSystem.None;
            }

            RepositoryRoot = string.IsNullOrEmpty(RepositoryRoot) ? Path.Combine(Directory.GetCurrentDirectory(), RepositoryName) : RepositoryRoot;

            if (Directory.Exists(RepositoryRoot))
            {
                throw new CommandLineException($"Directory already exists: {RepositoryRoot}.");
            }

            _console.WriteLine($"Creating .NET Project Repository at {RepositoryRoot} ...");
            Directory.CreateDirectory(RepositoryRoot);

            // Put a sln there
            await dotnet.Arguments("new", "sln")
                .InDirectory(RepositoryRoot)
                .ExecuteAsync(throwOnFailure: true);

            // Make a 'src' dir
            var srcDir = Path.Combine(RepositoryRoot, "src");
            Directory.CreateDirectory(srcDir);

            // Make a dir for the project
            var projectDir = Path.Combine(srcDir, RepositoryName);
            Directory.CreateDirectory(projectDir);

            // Stick a lib there
            await dotnet.Arguments("new", "classlib")
                .InDirectory(projectDir)
                .ExecuteAsync(throwOnFailure: true);

            // Add it to the sln
            await dotnet.Arguments("sln", "add", projectDir)
                .InDirectory(RepositoryRoot)
                .ExecuteAsync(throwOnFailure: true);

            // Do version control things
            if (!await vcs.TryInitializeAsync(RepositoryRoot))
            {
                _console.Error.WriteLine("Failed to initialize version control repository.");
                return 1;
            }

            return 0;
        }
    }
}
