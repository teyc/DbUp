using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DbUp.Engine;
using DbUp.Engine.Transactions;

namespace DbUp.PowerShell
{
    public class PowerShellFileSystemScriptProvider : IScriptProvider
    {
        private readonly string directoryPath;
        private readonly Func<string, bool> filter;
        private readonly Encoding encoding;

        public PowerShellFileSystemScriptProvider(string directoryPath)
        {
            this.directoryPath = directoryPath;
            encoding = Encoding.UTF8;
        }

        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var files = Directory.GetFiles(directoryPath, "*.ps1").AsEnumerable();
            if (filter != null)
            {
                files = files.Where(filter);
            }
            return files.Select(x => SqlScript.FromFile(x, encoding)).ToList();
        }

    }
}