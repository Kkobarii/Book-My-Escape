using System;
using System.Collections.Generic;
using System.Linq;
namespace DataLayer
{
    public class SqlLog
    {
        public static void Log(string type, string sql, Dictionary<string, object> parameters)
        {
            if (!GlobalConfig.LogConsole && !GlobalConfig.LogFile)
                return;
            
            foreach (KeyValuePair<string, object> param in parameters)
            {
                sql = sql.Replace(param.Key, param.Value.ToString());
            }
            
            if (GlobalConfig.LogConsole)
            {
                Console.WriteLine($"Executing {type}:");
                Console.WriteLine(sql);
            }
            if (GlobalConfig.LogFile)
            {
                using (StreamWriter file = new StreamWriter(GlobalConfig.LogFilePath, true))
                {
                    file.WriteLine($"Executing {type}:");
                    file.WriteLine(sql);
                }
            }
        }
        
        public static void ClearFile()
        {
            File.WriteAllText(GlobalConfig.LogFilePath, string.Empty);
        }
    }
}
