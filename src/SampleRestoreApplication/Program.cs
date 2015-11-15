using System;
using DbUp;

namespace SampleRestoreApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = @"Server=(LocalDB)\v11.0;Database=SampleRestoreApplication;Integrated Security=True;";
            var backupFilePath = "SampleRestoreApplication.bak";
            RestoreDatabase.For.SqlDatabase(connectionString, backupFilePath, false);
        }
    }
}
