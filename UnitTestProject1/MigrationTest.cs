using System;
using ACSDining.Infrastructure.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class MigrationTest
    {
        [TestMethod]
        public void TestMigration()
        {
            string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                           @"ACSDining.Core\DBinitial\DishDetails.xml";
            ApplicationDbContext context = ApplicationDbContext.Create();
            ApplicationDbInitializer.InitializeIdentityForEF(ApplicationDbContext.Create(), _path);
            Assert.IsNotNull(context.DishQuantities);
        }
    }
}
