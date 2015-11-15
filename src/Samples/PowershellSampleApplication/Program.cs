using System;
using System.Diagnostics;
using DbUp;
using DbUp.PowerShell;

namespace PowerShellSampleApplication
{
    class Program
    {
        static void Main()
        {
            var upgradeEngine = DeployChanges.To
                .PowerShell()
                .JournalTo(new FileSystemJournal("SchemaVersions"))
                .LogToConsole()                
                .WithPowerShellScriptsFromFileSystem("Scripts")
                .Build();

            var result = upgradeEngine.PerformUpgrade();

            if (!result.Successful)
            {
                Debugger.Break();
            }

            Console.Out.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
