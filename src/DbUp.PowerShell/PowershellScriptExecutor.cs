using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
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

            log.WriteInformation("Executing PowerShell script '{0}'", script.Name);

            var contents = script.Contents;

            var connectionManager = connectionManagerFactory();
            var scriptStatements = connectionManager.SplitScriptIntoCommands(contents);
            var index = -1;
            try
            {

                using (var powershell = System.Management.Automation.PowerShell.Create())
                {
                    scriptStatements
                        .ToList()
                        .ForEach(_ => powershell.Commands.AddScript(_));

                    powershell.Streams.Error.DataAdding += LogWith(log.WriteError);
                    powershell.Streams.Debug.DataAdding += LogWith(log.WriteInformation);
                    powershell.Streams.Warning.DataAdding += LogWith(log.WriteWarning);
                    powershell.Streams.Verbose.DataAdding += LogWith(log.WriteInformation);

                    var outputStream = new PSDataCollection<PSObject>();
                    outputStream.DataAdding += LogWith(log.WriteInformation);
                    powershell.Invoke(null, outputStream);

                }
            }
            catch (RuntimeException exception)
            {
                log.WriteError(exception.ErrorRecord.ToString());
                throw;
            }
        }

        private EventHandler<DataAddingEventArgs> LogWith(Action<string, object[]> log)
        {
            return (o, e) =>
            {
                var informationalRecord = e.ItemAdded as InformationalRecord;
                var errorRecord = e.ItemAdded as ErrorRecord;
                var message = informationalRecord != null ? informationalRecord.Message :
                    errorRecord != null ? errorRecord.ErrorDetails.Message :
                    e.ItemAdded.ToString();

                log(message, null);
            };
        }

        public void VerifySchema()
        {
            
        }

        public int? ExecutionTimeoutSeconds { get; set; }
    }
}