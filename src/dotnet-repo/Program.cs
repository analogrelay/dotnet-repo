using System;
using System.Diagnostics;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNet.Repo
{
    [Command(Name = Name, Description = Description)]
    [Subcommand(NewCommand.Name, typeof(NewCommand))]
    internal class Program
    {
        public const string Name = "dotnet-repo";
        public const string Description = "Tools to manage a .NET repository";
        private static int Main(string[] args)
        {
#if DEBUG
            if (args.Any(a => a == "--debug"))
            {
                args = args.Where(a => a != "--debug").ToArray();
                Console.WriteLine($"Ready for debugger to attach. Process ID: {Process.GetCurrentProcess().Id}.");
                Console.WriteLine("Press ENTER to continue.");
                Console.ReadLine();
            }
#endif

            // Configure services
            var services = new ServiceCollection()
                .AddLogging(logging =>
                {
                    logging.AddCliConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .BuildServiceProvider();
            var app = new CommandLineApplication<Program>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            try
            {
                return app.Execute(args);
            }
            catch (CommandLineException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        public int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 0;
        }
    }
}
