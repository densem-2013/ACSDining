using System.Collections.Generic;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Services;
using ACSDining.Web.Areas.EmployeeArea.Controllers;
using ACSDining.Web.Areas.SU_Area.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class TestApiController
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMenuForWeekService _menuForWeekService;
        private readonly IDishService _dishService;

        public TestApiController()
        {
            _unitOfWork = new UnitOfWork();
            _menuForWeekService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _dishService = new DishService(_unitOfWork.RepositoryAsync<Dish>());
        }

        [TestMethod]
        public void WeekMenuTestApi()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork);
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 18,
                Year = 2016
            };
            WeekMenuDto wmDto = weekMenuApi.GetWeekMenu(wyDto).Result;
            Assert.IsNotNull(wmDto);
        }

        [TestMethod]
        public void GetCategoriesTest()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork);
            string[] categories = weekMenuApi.GetCategories().Result;

            Assert.IsNotNull(categories);
        }

        [TestMethod]
        public void GetOrders()
        {
            OrdersController ord = new OrdersController(_unitOfWork);
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 17,
                Year = 2016
            };
            WeekOrderDto uwoDtos = ord.GetFactMenuOrders(wyDto,7,1).Result;

            Assert.IsNotNull(uwoDtos);

        }

        [TestMethod]
        public void GetNextWeekMenu()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork);
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 17,
                Year = 2016
            };
            WeekMenuDto dto = weekMenuApi.GetWeekMenu(wyDto).Result;

            Assert.IsNotNull(dto);

        }

        [TestMethod]
        public void TestWeekMenuDelete()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork);
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 18,
                Year = 2016
            };
            MenuForWeek weekmenu = _menuForWeekService.GetWeekMenuByWeekYear(wyDto);
            if (weekmenu == null)
            {
                WeekMenuDto dto = weekMenuApi.GetWeekMenu(wyDto).Result;
                weekmenu = _menuForWeekService.GetWeekMenuByWeekYear(wyDto);
            }
            var res = weekMenuApi.DeleteMenuForWeek(weekmenu.ID).Result;
            weekmenu = _menuForWeekService.GetWeekMenuByWeekYear(wyDto);

            Assert.IsNull(weekmenu);

        }

        [TestMethod]
        public void EmployeDtoGetTest()
        {
            EmployeeOrderApiController emplorderapi=new EmployeeOrderApiController(_unitOfWork);
        }
    }
}
