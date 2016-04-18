using System;
using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;
using ACSDining.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class MigrationTest
    {
        private readonly ApplicationDbContext dataContext;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IWorkDaysService _workDaysService;
        private readonly ApplicationUserManager _userManager;

        public MigrationTest()
        {
            _unitOfWork = new UnitOfWork();
            dataContext = _unitOfWork.GetContext();
            IRepositoryAsync<WorkingWeek> workRepo = _unitOfWork.RepositoryAsync<WorkingWeek>();
            _workDaysService = new WorkDaysService(workRepo);
            _userManager = new ApplicationUserManager(new UserStore<User>(dataContext));
        }

        [TestMethod]
        public void TestMigration()
        {
            string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                           @"ACSDining.Core\DBinitial\DishDetails.xml";

            ApplicationDbInitializer.InitializeIdentityForEf(dataContext, _path);

            ApplicationDbInitializer.CreateWorkingDays(dataContext);

            var dishes = ApplicationDbInitializer.GetDishesFromXml(dataContext, _path);

            ApplicationDbInitializer.CreateMenuForWeek(dataContext, dishes);

            _path = _path.Replace(@"DishDetails", "Employeers");

            ApplicationDbInitializer.GetUsersFromXml(dataContext, _path);
            ApplicationDbInitializer.CreateOrders(dataContext);

            List<DishQuantity> dqualist = dataContext.DishQuantities.ToList();
            Assert.IsTrue(dqualist.Count > 0);
        }

        [TestMethod]
        public void CreateWorkDaysTest()
        {
            ApplicationDbInitializer.CreateWorkingDays(dataContext);
            List<WorkingDay> workingDays = _workDaysService.Queryable().SelectMany(ww => ww.WorkingDays).ToList();
            Assert.IsTrue(workingDays.Count > 0);

        }

        [TestMethod]
        public void CreateAdminUsers()
        {
            ApplicationDbInitializer.AddUser(_userManager,dataContext);
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dataContext));
            IdentityRole admrole = roleManager.FindByName("Administrator");
            List<User> users = _userManager.Users.Where(u=>u.Roles.Any(r=>r.RoleId==admrole.Id)).ToList();
            Assert.IsTrue(users.Count > 0);

        }
    }
}
