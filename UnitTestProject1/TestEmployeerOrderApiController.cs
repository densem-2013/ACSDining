using System;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class TestEmployeerOrderApiController
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ApplicationUserManager _userManager;
        private readonly IOrderMenuService _weekOrderMenuService;
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IWeekPaimentService _weekPaimentService;

        public TestEmployeerOrderApiController()
        {
            _unitOfWork = new UnitOfWork(new ApplicationDbContext());
            _userManager = new ApplicationUserManager(new UserStore<User>(_unitOfWork.GetContext()));
            _weekOrderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _weekPaimentService = new WeekPaimentService(_unitOfWork.RepositoryAsync<WeekPaiment>());
        }

        [TestMethod]
        public void TestDeleteWeekOrder()
        {
            User user = _userManager.FindByName("employee");
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 19,
                Year = 2016
            };
            WeekOrderMenu weekOrderMenu = _weekOrderMenuService.FindByUserIdWeekYear(user.Id, wyDto);
            _weekOrderMenuService.Delete(weekOrderMenu);
            weekOrderMenu = _weekOrderMenuService.FindByUserIdWeekYear(user.Id, wyDto);
            Assert.IsNull(weekOrderMenu);
        }

        [TestMethod]
        public void TestGetUserWeekOrder()
        {
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 19,
                Year = 2016
            };
            User user = _userManager.FindByName("employee");
            //WeekOrderMenu ordmenu = _weekOrderMenuService.FindByUserIdWeekYear(user.Id, wyDto);
            WeekPaiment weekPaiment = _weekPaimentService.GetByUseridWeekYear(user.Id, wyDto);
            EmployeeWeekOrderDto model = EmployeeWeekOrderDto.MapDto(_unitOfWork.GetContext(), weekPaiment, wyDto);
            Assert.IsNotNull(model);
        }
    }
}
