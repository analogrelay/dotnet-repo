using Microsoft.Extensions.Logging;

namespace DotNet.Repo
{
    public class ToolSet
    {
        private Tool _dotnet;
        private Tool _git;

        public Tool DotNet => _dotnet ?? throw new CommandLineException("Unable to locate 'dotnet' executable!");
        public Tool Git => _git ?? throw new CommandLineException("Unable to locate 'git' executable!");

        public ToolSet(ILoggerFactory loggerFactory)
        {
            _dotnet = Tool.Locate("dotnet", loggerFactory);
            _git = Tool.Locate("git", loggerFactory);
        }
    }
}
