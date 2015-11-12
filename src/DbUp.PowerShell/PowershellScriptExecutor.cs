using System;
using System.Collections.Generic;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace DbUp.PowerShell
{
    public class PowerShellScriptExecutor : IScriptExecutor
    {
        private readonly Func<IConnectionManager> connectionManagerFactory; 
        private readonly Func<IUpgradeLog> upgradeLogFactory;

        public PowerShellScriptExecutor(Func<IUpgradeLog> upgradeLogFactory, Func<IConnectionManager> connectionManagerFactory)
        {
            this.upgradeLogFactory = upgradeLogFactory;
            this.connectionManagerFactory = connectionManagerFactory;
        }

        public void Execute(SqlScript script)
        {
            var emptyDictionary = new Dictionary<string, string>();
            Execute(script, emptyDictionary);
        }

        public void Execute(SqlScript script, IDictionary<string, string> variables)
        {
            var log = upgradeLogFactory();

            log.WriteInformation("Executing Powershell script '{0}'", script.Name);

            var contents = script.Contents;

            var connectionManager = connectionManagerFactory();
            var scriptStatements = connectionManager.SplitScriptIntoCommands(contents);
            var index = -1;

            using (var powershell = PowerShell.Create())
            {
                foreach (var scriptStatement in scriptStatements)
                {
                    powershell.Commands.Clear();
                    powershell.Commands.AddScript(scriptStatement);
                    var output = powershell.Invoke();
                    foreach (var psObject in output)
                    {
                        log.WriteInformation(psObject.ToString());
                    }
                }
            }
        }

        public void VerifySchema()
        {
            
        }

        public int? ExecutionTimeoutSeconds { get; set; }
    }
}