using System;
using DbUp.Builder;

namespace DbUp.PowerShell
{
    public static class PowerShellExtensions
    {
        public static UpgradeEngineBuilder WithPowerShellScriptsFromFileSystem(this UpgradeEngineBuilder builder, string path)
        {
            return builder.WithScripts(new PowerShellFileSystemScriptProvider(path));
        }

        public static UpgradeEngineBuilder Powershell(this SupportedDatabases supported)
        {
            var builder = new UpgradeEngineBuilder();
            builder.Configure(c => c.ScriptExecutor = new PowerShellScriptExecutor(() => c.Log, () => c.ConnectionManager));
            builder.Configure(c => c.Journal = new FileSystemJournal("SchemaVersions"));
            builder.Configure(c => c.ConnectionManager = new PowerShellConnnectionManager());
            return builder;
        }
    }
}