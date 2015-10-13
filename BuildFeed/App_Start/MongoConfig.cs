using System.Configuration;

namespace BuildFeed
{
   internal static class MongoConfig
   {
      public static string Host { get; private set; }
      public static int Port { get; private set; }
      public static string Database { get; private set; }

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
      }
   }
}