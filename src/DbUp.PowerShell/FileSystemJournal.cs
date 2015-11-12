using System;
using System.IO;
using DbUp.Engine;

namespace DbUp.PowerShell
{
    public class FileSystemJournal : IJournal
    {
        private readonly string directoryPath;

        public FileSystemJournal(string directoryPath)
        {
            this.directoryPath = directoryPath;
        }

        public string[] GetExecutedScripts()
        {
            CreateDirectory();

            return Directory.GetFileSystemEntries(directoryPath);
        }

        public void StoreExecutedScript(SqlScript script)
        {
            CreateDirectory();

            var filePath = Path.Combine(directoryPath, script.Name);
            File.WriteAllText(filePath, script.Contents);
        }

        private void CreateDirectory()
        {
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        }
    }
}