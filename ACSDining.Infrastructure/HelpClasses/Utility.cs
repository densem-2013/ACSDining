using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using ACSDining.Infrastructure.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using User = ACSDining.Core.Domains.User;

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

                var _path = path.Replace("userdefinedtypes", "storedfunc");
                file = new FileInfo(_path);
                script = file.OpenText().ReadToEnd();
                server.ConnectionContext.ExecuteNonQuery(script);

                _path = _path.Replace("storedfunc", "storedproc");
                file = new FileInfo(_path);
                script = file.OpenText().ReadToEnd();
                server.ConnectionContext.ExecuteNonQuery(script);

                _path = _path.Replace("storedproc", "storedtriggers");
                file = new FileInfo(_path);
                script = file.OpenText().ReadToEnd();
                server.ConnectionContext.ExecuteNonQuery(script);
            }
        }

        public static void CreateUsersForReport(ApplicationDbContext context, string usersstring)
        {
            var userManager = new ApplicationUserManager(new UserStore<User>(context));
            string[][] userarray = usersstring.Split(',').Select(str => str.Split(' ')).ToArray();
            foreach (var strar in userarray)
            {
                User usersu = userManager.FindByName(strar[0] + " " + strar[1]);
                if (usersu == null)
                {
                    usersu = new User
                    {
                        UserName = strar[0] + " " + strar[1],
                        FirstName = strar[1],
                        LastName = strar[0],
                        SecurityStamp = Guid.NewGuid().ToString(),
                        LastLoginTime = DateTime.UtcNow,
                        RegistrationDate = DateTime.UtcNow,
                        PasswordHash =
                            userManager.PasswordHasher.HashPassword("12345")
                    };
                    IdentityRole role = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "Employee"));
                    if (role != null) usersu.Roles.Add(new IdentityUserRole {RoleId = role.Id, UserId = usersu.Id});
                    context.Entry(usersu).State = EntityState.Added;
                }
            }
            context.SaveChanges();
        }
    }
}
