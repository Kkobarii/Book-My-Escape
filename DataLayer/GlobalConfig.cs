namespace DataLayer
{
    public class GlobalConfig
    {
        public static string DatabasePath { get; set; } = "database.db;";
        public static bool LogConsole { get; set; } = true;
        public static bool LogFile { get; set; } = true;
        public static string LogFilePath { get; set; } = "log.txt";
    }
}
