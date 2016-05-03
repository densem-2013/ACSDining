using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace ACSDining.Infrastructure.HelpClasses
{
    public static class Utility
    {
        public static void CreateStoredFuncs(string path)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ApplicationDbContext"].ConnectionString;


            //string path = Environment.CurrentDirectory.Replace(@"ACSDining.Infrastructure\bin\Debug", "") +
            //                          @"ACSDining.Web\App_Data\DBinitial\storedfunc.sql";

            //var path = Environment.CurrentDirectory.Replace("bin\\Debug", "Sql\\instnwnd.sql");
            var file = new FileInfo(path);
            var script = file.OpenText().ReadToEnd();

            using (var connection = new SqlConnection(connectionString))
            {
                var server = new Server(new ServerConnection(connection));
                server.ConnectionContext.ExecuteNonQuery(script);
            }
        }
    }
}
