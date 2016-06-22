using System;
using System.Data.Entity.Migrations;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"ACSDining.Infrastructure\bin\Debug", "") +
                                     @"ACSDining.Web\App_Data\DBinitial\DishDetails.xml";

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            ApplicationDbInitializer.InitializeIdentityForEf(context, _path);
            var dishes = ApplicationDbInitializer.GetDishesFromXml(context, _path);
            ApplicationDbInitializer.CreateWorkingDays(context);
            ApplicationDbInitializer.CreateMenuForWeek(context, dishes);
            _path = _path.Replace(@"DishDetails", "Employeers");
             ApplicationDbInitializer.GetUsersFromXml(context, _path);
            ApplicationDbInitializer.CreateOrders(context);
            _path = _path.Replace(@"Employeers.xml", "storedfunc.sql");
            Utility.CreateStoredFuncs(_path);

        }

    }
}
