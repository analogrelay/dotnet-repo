using System.IO;
using System.Threading.Tasks;

namespace DotNet.Repo
{
    internal static class ResourceFiles
    {
        public static async Task<string> LoadResourceFile(params string[] path)
        {
            var name = string.Join(".", path);
            using (var stream = typeof(ResourceFiles).Assembly.GetManifestResourceStream($"DotNet.Repo.Resources.{name}"))
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
