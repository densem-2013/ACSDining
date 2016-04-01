using System;
using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.DataContext;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;
using ACSDining.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class MigrationTest
    {
        private readonly ApplicationDbContext dataContext;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IWorkDaysService _workDaysService;
        private readonly IUserAccountService _userService;

        public MigrationTest()
        {
            dataContext = new ApplicationDbContext();
            _unitOfWork = new UnitOfWork(dataContext);
            IRepositoryAsync<WorkingWeek> workRepo = _unitOfWork.RepositoryAsync<WorkingWeek>();
            _workDaysService = new WorkDaysService(workRepo);
            IRepositoryAsync<User> userRepo = _unitOfWork.RepositoryAsync<User>();
            _userService = new UserAccountService(userRepo);
        }

        [TestMethod]
        public void TestMigration()
        {
            string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                           @"ACSDining.Core\DBinitial\DishDetails.xml";
            //ApplicationDbContext context = new ApplicationDbContext();
            ApplicationDbInitializer.InitializeIdentityForEF(dataContext, _path);
            Assert.IsNotNull(dataContext.DishQuantities);
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
            ApplicationDbInitializer.AddUser(dataContext);
            List<User> users = _userService.Queryable().ToList();
            Assert.IsTrue(users.Count > 0);

        }
    }
}
