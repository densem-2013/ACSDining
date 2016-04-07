﻿////using ACSDining.Core.DataContext;

using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Service;
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
        public  void WeekMenuTestApi()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork);
            WeekMenuDto wmDto = weekMenuApi.GetWeekMenu(12, 2016).Result;
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
            OrdersDto ordermenus = ord.GetMenuOrders(15, 2016).Result;

            Assert.IsNotNull(ordermenus);

        }
    }
}
