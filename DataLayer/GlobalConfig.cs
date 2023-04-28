namespace DataLayer
{
    public class GlobalConfig
    {
        public static string ConnectionString { get; set; } = "Data Source=database.db;";
        public static bool LogConsole { get; set; } = true;
        public static bool LogFile { get; set; } = false;
        public static string LogFilePath { get; set; } = "log.txt";
    }
}
