using System.Configuration;
using BuildFeed.Models;

namespace BuildFeed
{
   internal static class MongoConfig
   {
      public static string Host { get; }
      public static int Port { get; }
      public static string Database { get; }

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

      public static void SetupIndexes()
      {
         Build b = new Build();
         b.SetupIndexes();
      }
   }
}