using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Automation.Language;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace DbUp.PowerShell
{
    public class PowerShellConnnectionManager : IConnectionManager
    {
        public IDisposable OperationStarting(IUpgradeLog upgradeLog, List<SqlScript> executedScripts)
        {
            return null;
        }

        public void ExecuteCommandsWithManagedConnection(Action<Func<IDbCommand>> action)
        {
            
        }

        public T ExecuteCommandsWithManagedConnection<T>(Func<Func<IDbCommand>, T> actionWithResult)
        {
            return actionWithResult(() => new PowerShellCommand());
        }

        public TransactionMode TransactionMode { get; set; }

        public bool IsScriptOutputLogged { get; set; }

        public IEnumerable<string> SplitScriptIntoCommands(string scriptContents)
        {
            Token[] tokens;
            ParseError[] parseError;
            var scriptBlock = Parser.ParseInput(scriptContents, out tokens, out parseError);

            var astVisitor = new Visitor();
            scriptBlock.Visit(astVisitor);
            return astVisitor.GetCommands();
        }

        public bool TryConnect(IUpgradeLog upgradeLog, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        private class Visitor : AstVisitor
        {
            private readonly IList<string> scriptCommands = new List<string>();

            public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
            {
                scriptCommands.Add(functionDefinitionAst.ToString());
                return AstVisitAction.SkipChildren;
            }

            public override AstVisitAction VisitIfStatement(IfStatementAst ifStmtAst)
            {
                scriptCommands.Add(ifStmtAst.ToString());
                return AstVisitAction.SkipChildren;
            }

            public override AstVisitAction VisitStatementBlock(StatementBlockAst statementBlockAst)
            {
                scriptCommands.Add(statementBlockAst.ToString());
                return AstVisitAction.SkipChildren;
            }

            public override AstVisitAction VisitCommand(CommandAst commandAst)
            {
                scriptCommands.Add(commandAst.ToString());
                return AstVisitAction.SkipChildren;
            }

            public IEnumerable<string> GetCommands()
            {
                return scriptCommands;
            }

        }
    }
}