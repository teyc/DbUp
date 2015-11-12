using System;
using System.Diagnostics;
using DbUp;
using DbUp.PowerShell;

namespace PowershellSampleApplication
{
    class Program
    {
        static void Main()
        {
            var upgradeEngine = DeployChanges.To
                .Powershell()
                .JournalTo(new FileSystemJournal("SchemaVersions"))
                .LogToConsole()
                .WithPowerShellScriptsFromFileSystem("Scripts")
                .Build();

            var result = upgradeEngine.PerformUpgrade();

            if (!result.Successful)
            {
                Debugger.Break();
            }
        }
    }
}
