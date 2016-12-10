using System.Threading.Tasks;
using BuildFeed.Model;

namespace MigrateBuildStrings
{
   internal class Program
   {
      private static void Main(string[] args)
      {
         Task.Run(async () =>
         {
            BuildRepository bModel = new BuildRepository();
            foreach (Build build in await bModel.SelectBuildsByOrder())
            {
               await bModel.Update(build);
            }
         }).Wait();
      }
   }
}