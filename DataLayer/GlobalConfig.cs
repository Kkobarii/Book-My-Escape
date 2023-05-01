﻿namespace DataLayer
{
    public class GlobalConfig
    {
        public static string AssetsPath 
        { 
            get
            {
                string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)!;

                path = path.Substring(0, path.IndexOf("SemestralProject\\") + 17);
                path = path.Substring(6);
                path += "Assets\\";

                return path;
            }
            set
            {
                AssetsPath = value;
            }
        }
        public static string DatabasePath { get; set; } = AssetsPath + "database.db";
        public static bool LogConsole { get; set; } = true;
        public static bool LogFile { get; set; } = true;
        public static string LogFilePath { get; set; } = AssetsPath + "log.txt";
    }
}
