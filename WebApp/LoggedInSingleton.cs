using DataLayer.Models;

namespace WebApp
{
    public class LoggedInSingleton
    {
        private static LoggedInSingleton? _instance;
        private LoggedInSingleton()
        {
        }
        public static LoggedInSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoggedInSingleton();
                }
                return _instance;
            }
        }
        public User? LoggedInUser { get; set; }
    }
}
