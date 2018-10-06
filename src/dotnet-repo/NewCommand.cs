using System.IO;
using System.Threading.Tasks;
using DotNet.Repo.Build;
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
        private readonly ILogger<NewCommand> _logger;
        private readonly VersionControlManager _versionControlManager;
        private readonly SolutionManager _solutionManager;
        private readonly BuildSystem _buildSystem;
        private readonly StrongNameModule _strongNameModule;
        private readonly SourceLinkModule _sourceLinkModule;

        [Option("--vcs <VERSION_CONTROL_SYSTEM>", Description = "The version control system to use. Defaults to 'git', use 'none' to disable version control support.")]
        public string VcsType { get; set; }

        [Option("-p|--path <PATH>", Description = "The path in which to create the repository. Defaults to '[current directory]/[name]'.")]
        public string RepositoryRoot { get; set; }

        [Argument(0, "<NAME>", Description = "The name of the repository to create. Will also be used as the solution/project name by default")]
        public string RepositoryName { get; set; }

        public NewCommand(IConsole console, ILoggerFactory loggerFactory, VersionControlManager versionControlManager, SolutionManager solutionManager, BuildSystem buildSystem, StrongNameModule strongNameModule, SourceLinkModule sourceLinkModule)
        {
            _console = console;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<NewCommand>();
            _versionControlManager = versionControlManager;
            _solutionManager = solutionManager;
            _buildSystem = buildSystem;
            _strongNameModule = strongNameModule;
            _sourceLinkModule = sourceLinkModule;
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
            var vcs = _versionControlManager.GetVersionControlSystem(string.IsNullOrEmpty(VcsType) ? "git" : VcsType);
            if (vcs == null)
            {
                throw new CommandLineException($"Unknown version control system: {VcsType}");
            }

            if (!vcs.IsInstalled)
            {
                _logger.LogWarning("Version control system '{VcsType}' is not installed.", VcsType);
                vcs = VersionControlSystem.None;
            }

            RepositoryRoot = string.IsNullOrEmpty(RepositoryRoot) ? Path.Combine(Directory.GetCurrentDirectory(), RepositoryName) : RepositoryRoot;

            if (Directory.Exists(RepositoryRoot))
            {
                throw new CommandLineException($"Directory already exists: {RepositoryRoot}.");
            }

            _logger.LogInformation("Creating .NET Project Repository at {RepositoryRoot} ...", RepositoryRoot);
            Directory.CreateDirectory(RepositoryRoot);

            // Put a sln there
            var solutionPath = Path.Combine(RepositoryRoot, $"{RepositoryName}.sln");
            await _solutionManager.CreateSolutionAsync(solutionPath);

            // Add a new class library to the solution
            await _solutionManager.AddNewProjectAsync(solutionPath, "src", RepositoryName, "classlib");

            // Configure the build
            await _buildSystem.InitializeAsync(RepositoryRoot);

            // Install Modules
            await _strongNameModule.InstallAsync(Path.Combine(RepositoryRoot, "build", "modules", "StrongName"));
            await _sourceLinkModule.InstallAsync(Path.Combine(RepositoryRoot, "build", "modules", "SourceLink"));

            // Do version control things
            await vcs.InitializeAsync(RepositoryRoot);

            // Commit the changes
            await vcs.CommitAsync(RepositoryRoot, "Initial template");

            return 0;
        }
    }
}
