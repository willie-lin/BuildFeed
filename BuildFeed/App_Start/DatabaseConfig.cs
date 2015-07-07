using System.Configuration;

namespace BuildFeed
{
    public static class DatabaseConfig
    {
        public static string Host { get; private set; }
        public static int Port { get; private set; }
        public static long Database { get; private set; }

        static DatabaseConfig()
        {
            Host = ConfigurationManager.AppSettings["data:ServerHost"];

            int port;
            bool success = int.TryParse(ConfigurationManager.AppSettings["data:ServerPort"], out port);
            if (!success)
            {
                port = 6379; // redis default port
            }
            Port = port;

            long db;
            success = long.TryParse(ConfigurationManager.AppSettings["data:ServerDB"], out db);
            if (!success)
            {
                db = 0; // redis default db
            }
            Database = db;
        }
    }
}