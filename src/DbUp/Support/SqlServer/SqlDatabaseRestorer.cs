using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DbUp.Support.SqlServer
{
    internal class SqlDatabaseRestorer
    {
        internal void RestoreFromDiskOverwrite(string masterConnectionString, string actualDatabaseName,
            string backupFilePath)
        {
            using (var conn = new SqlConnection(masterConnectionString))
            {
                conn.Open();

                var fileList = GetFileList(conn, backupFilePath).ToList();

                DropDatabaseIfExists(conn, actualDatabaseName);

                RestoreDatabase(conn, actualDatabaseName, backupFilePath, fileList);
            }
        }

        internal void RestoreFromDiskIfAbsent(string masterConnectionString, string actualDatabaseName, string backupFilePath)
        {
            using (var conn = new SqlConnection(masterConnectionString))
            {
                conn.Open();

                if (DatabaseExists(conn, actualDatabaseName))
                    return;

                var fileList = GetFileList(conn, backupFilePath).ToList();
                RestoreDatabase(conn, actualDatabaseName, backupFilePath, fileList);
            }
        }

        private bool DatabaseExists(SqlConnection connection, string actualDatabaseName)
        {
            var stmt = string.Format("SELECT COUNT(*) AS NUMBER FROM sys.databases WHERE NAME = N'{0}'", actualDatabaseName.SafeSql());
            Func<SqlDataReader, int> toCount = reader => reader.GetInt32(0);
            return ExecuteQuery(connection, stmt, toCount).Single() != 0;

        }

        private IEnumerable<FileList> GetFileList(SqlConnection conn, string backupFilePath)
        {
            var stmt = String.Format("restore filelistonly from disk = N'{0}' WITH FILE=1",
                backupFilePath.SafeSql());

            Func<SqlDataReader, FileList> tofileList = reader => new FileList(
                (string) reader["LogicalName"],
                (string) reader["PhysicalName"],
                (string) reader["Type"]);

            return ExecuteQuery(conn, stmt, tofileList);
        }

        private void DropDatabaseIfExists(SqlConnection conn, string actualDatabaseName)
        {
            var stmt = @"
                    IF EXISTS(select * from sys.databases where name='yourDBname')
                    BEGIN
                        ALTER DATABASE [yourDBname] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
                        DROP DATABASE [yourDBname]
                    END
                "
                .Replace("yourDBname", actualDatabaseName.SafeSql());

            ExecuteNonQuery(conn, stmt);
        }

        private static void ExecuteNonQuery(SqlConnection conn, string stmt)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandTimeout = 0;
                cmd.CommandText = stmt;
                cmd.ExecuteNonQuery();
            }
        }

        private void RestoreDatabase(SqlConnection conn, string actualDatabaseName, string backupFilePath, IEnumerable<FileList> fileList)
        {
            string dataPath, logPath;
            GetDataAndLogPath(conn, out dataPath, out logPath);

            var fileLists = fileList as FileList[] ?? fileList.ToArray();
            var dataFile = fileLists.Single(file => file.Type == "D");
            var moveData = string.Format("MOVE '{0}' TO '{1}\\{2}.mdf'", dataFile.LogicalName.SafeSql(), dataPath, actualDatabaseName);

            var logFile = fileLists.Single(file => file.Type == "L");
            var moveLog = string.Format("MOVE '{0}' TO '{1}\\{2}.ldf'", logFile.LogicalName.SafeSql(), logPath, actualDatabaseName);

            var stmt = @"
                    RESTORE DATABASE [yourDBname] FROM DISK = N'yourDBpath' 
                    WITH FILE = 1, NOUNLOAD, STATS = 5, REPLACE,
                    {{moveData}},
                    {{moveLog}}                    

                    "
                .Replace("yourDBname", actualDatabaseName.SafeSql())
                .Replace("yourDBpath", backupFilePath)
                .Replace("{{moveData}}", moveData)
                .Replace("{{moveLog}}", moveLog)
                ;

            ExecuteNonQuery(conn, stmt);
        }

        private void GetDataAndLogPath(SqlConnection conn, out string dataPath, out string logPath)
        {
            dataPath = string.Empty;
            logPath = string.Empty;

            var stmt = "SELECT serverproperty('InstanceDefaultDataPath') AS InstanceDefaultDataPath, serverproperty('InstanceDefaultLogPath') As InstanceDefaultLogPath";

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = stmt;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dataPath = (string) reader["InstanceDefaultDataPath"];
                        logPath = (string) reader["InstanceDefaultLogPath"];
                    }
                }
            }
        }

        private static IEnumerable<T> ExecuteQuery<T>(SqlConnection conn, string stmt, Func<SqlDataReader, T> tofileList)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = stmt;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return tofileList(reader);
                    }
                }
            }
        }

        private class FileList
        {
            public FileList(string logicalName, string physicalName, string type)
            {
                LogicalName = logicalName;
                PhysicalName = physicalName;
                Type = type;
            }

            public string LogicalName { get; private set; }
            public string PhysicalName { get; private set; }
            public string Type { get; private set; }
        }
    }

    internal static class SqlExtensions
    {
        public static string SafeSql(this string str)
        {
            return str.Replace("'", "''");
        }
    }
}