using System;

namespace ZeitPlaner.Data
{
    /// <summary>
    /// Constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The zeitplaner path.
        /// </summary>
        public static string ZeitplanerPath = System.IO.Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "Zeitplaner");

        /// <summary>
        /// The database name.
        /// </summary>
        private static string DatabaseName = "zeitplaner.db";

        /// <summary>
        /// The database file path.
        /// </summary>
        public static string DatabaseFilePath = System.IO.Path.Combine(ZeitplanerPath, DatabaseName);
    }
}