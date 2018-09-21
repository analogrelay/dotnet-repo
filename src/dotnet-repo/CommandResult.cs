using System.Collections.Generic;

namespace DotNet.Repo
{
    public class CommandResult
    {
        public int ExitCode { get; }
        public IReadOnlyList<string> StandardOutput { get; }
        public IReadOnlyList<string> StandardError { get; }
        public bool Success => ExitCode == 0;

        public CommandResult(int exitCode, IReadOnlyList<string> standardOutput, IReadOnlyList<string> standardError)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }
    }
}
