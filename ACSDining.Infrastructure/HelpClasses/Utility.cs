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

            var file = new FileInfo(path);
            var script = file.OpenText().ReadToEnd();

            using (var connection = new SqlConnection(connectionString))
            {
                var server = new Server(new ServerConnection(connection));
                server.ConnectionContext.ExecuteNonQuery(script);

                var _path = path.Replace("storedfunc", "storedproc");
                file = new FileInfo(_path);
                script = file.OpenText().ReadToEnd();
                server.ConnectionContext.ExecuteNonQuery(script);

                _path = _path.Replace("storedproc", "storedtriggers");
                file = new FileInfo(_path);
                script = file.OpenText().ReadToEnd();
                server.ConnectionContext.ExecuteNonQuery(script);
            }
        }
    }
}
