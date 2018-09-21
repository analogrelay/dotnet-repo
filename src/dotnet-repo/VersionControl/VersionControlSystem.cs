using System.Threading.Tasks;

namespace DotNet.Repo.VersionControl
{
    public abstract class VersionControlSystem
    {
        public static readonly VersionControlSystem None = NoVersionControlSystem.Instance;

        public abstract string Name { get; }
        public abstract bool IsInstalled { get; }

        public abstract Task<bool> TryInitializeAsync(string repositoryRoot);

        private class NoVersionControlSystem : VersionControlSystem
        {
            public static readonly VersionControlSystem Instance = new NoVersionControlSystem();

            public override bool IsInstalled => true;
            public override string Name => "none";

            private NoVersionControlSystem()
            {
            }

            public override Task<bool> TryInitializeAsync(string repositoryRoot)
            {
                // Do nothing
                return Task.FromResult(true);
            }
        }
    }
}
