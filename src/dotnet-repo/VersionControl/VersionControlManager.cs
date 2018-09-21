using System.Collections.Generic;
using System.Linq;

namespace DotNet.Repo.VersionControl
{
    public class VersionControlManager
    {
        private readonly Dictionary<string, VersionControlSystem> _versionControlSystems;

        public VersionControlManager(IEnumerable<VersionControlSystem> versionControlSystems)
        {
            _versionControlSystems = versionControlSystems.ToDictionary(v => v.Name);
            _versionControlSystems["none"] = VersionControlSystem.None;
        }

        public virtual VersionControlSystem GetVersionControlSystem(string name)
        {
            if (_versionControlSystems.TryGetValue(name, out var vcs))
            {
                return vcs;
            }
            return null;
        }
    }
}
