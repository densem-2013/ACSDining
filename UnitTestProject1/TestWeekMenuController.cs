using System;
using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Web.Areas.SU_Area.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class TestWeekMenuController
    {
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekMenuService;
        private readonly ApplicationUserManager _userManager;

        public TestWeekMenuController()
        {
            _unitOfWork = new UnitOfWork(new ApplicationDbContext());
            _db = _unitOfWork.GetContext();
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _userManager = new ApplicationUserManager(new UserStore<User>(_unitOfWork.GetContext()));
        }

        [TestMethod]
        public void EmailTest()
        {
            List<User> empluserlist = new List<User>(new[]{_userManager.FindByNameAsync("employee").Result});
            string message = "Hello. This is Registration message";
            MessageService.SendEmailAsync(empluserlist, MessageTopic.Registration, DateTime.Now.ToShortDateString(), message);
            //MessageService.WatchdogMailMessage(new List<string>(new[]{_userManager.FindByNameAsync("employee").Result.Email}), message, "Admin Message Test");
            //Assert.Inconclusive("Ok");
        }

    }
}
