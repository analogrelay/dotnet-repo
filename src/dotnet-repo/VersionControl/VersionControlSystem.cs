using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Repo.VersionControl
{
    public abstract class VersionControlSystem
    {
        public static readonly VersionControlSystem None = NoVersionControlSystem.Instance;

        public abstract string Name { get; }
        public abstract bool IsInstalled { get; }

        public abstract Task InitializeAsync(string repositoryRoot, CancellationToken cancellationToken = default);
        public abstract Task CommitAsync(string repositoryRoot, string message, bool addAll = true, CancellationToken cancellationToken = default);

        private class NoVersionControlSystem : VersionControlSystem
        {
            public static readonly VersionControlSystem Instance = new NoVersionControlSystem();

            public override bool IsInstalled => true;
            public override string Name => "none";

            private NoVersionControlSystem()
            {
            }

            public override Task InitializeAsync(string repositoryRoot, CancellationToken cancellationToken = default)
            {
                // Do nothing
                return Task.CompletedTask;
            }

            public override Task CommitAsync(string repositoryRoot, string message, bool addAll = true, CancellationToken cancellationToken = default)
            {
                // Do nothing
                return Task.CompletedTask;
            }
        }
    }
}
