using System;
using System.Collections.Generic;
using System.Text;

namespace ZeitPlaner.Data
{
    public static class Constants
    {
        public static string ZeitplanerPath = System.IO.Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "Zeitplaner");

        private static string DatabaseName = "zeitplaner.db";

        public static string DatabaseFilePath = System.IO.Path.Combine(ZeitplanerPath, DatabaseName);
    }
}
