using System.Configuration;

namespace MongoAuth
{
    internal static class DatabaseConfig
    {
        public static string Host { get; }
        public static int Port { get; }
        public static string Database { get; }
        public static string Username { get; }
        public static string Password { get; }

        static DatabaseConfig()
        {
            Host = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["data:MongoHost"])
                ? ConfigurationManager.AppSettings["data:MongoHost"]
                : "localhost";

            bool success = int.TryParse(ConfigurationManager.AppSettings["data:MongoPort"], out int port);
            if (!success)
            {
                port = 27017; // mongo default port
            }
            Port = port;

            Database = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["data:MongoDB"])
                ? ConfigurationManager.AppSettings["data:MongoDB"]
                : "MongoAuth";

            Username = ConfigurationManager.AppSettings["data:MongoUser"] ?? "";
            Password = ConfigurationManager.AppSettings["data:MongoPass"] ?? "";
        }
    }
}