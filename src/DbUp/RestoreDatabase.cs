using System;

namespace DbUp
{
    public static class RestoreDatabase
    {
        private static readonly SupportedDatabasesForRestoreDatabase Instance = new SupportedDatabasesForRestoreDatabase();

        /// <summary>
        /// Returns the databases supported by DbUp's EnsureDatabase feature.
        /// </summary>
        public static SupportedDatabasesForRestoreDatabase For
        {
            get { return Instance; }
        }
    }
}
