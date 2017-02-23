using System.Configuration;

namespace BuildFeed.Model
{
    public static class MongoConfig
    {
        public static string Host { get; }
        public static int Port { get; }
        public static string Database { get; }
        public static string Username { get; }
        public static string Password { get; }

        static MongoConfig()
        {
            Host = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["data:MongoHost"])
                ? ConfigurationManager.AppSettings["data:MongoHost"]
                : "localhost";

            int _port;
            bool success = int.TryParse(ConfigurationManager.AppSettings["data:MongoPort"], out _port);
            if (!success)
            {
                _port = 27017; // mongo default port
            }
            Port = _port;

            Database = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["data:MongoDB"])
                ? ConfigurationManager.AppSettings["data:MongoDB"]
                : "MongoAuth";

            Username = ConfigurationManager.AppSettings["data:MongoUser"] ?? "";
            Password = ConfigurationManager.AppSettings["data:MongoPass"] ?? "";
        }

        public static void SetupIndexes()
        {
            BuildRepository b = new BuildRepository();
            b.SetupIndexes();
        }
    }
}