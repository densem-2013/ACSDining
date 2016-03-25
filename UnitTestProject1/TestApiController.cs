using System;
using ACSDining.Core.DataContext;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Identity;
using ACSDining.Service;
using ACSDining.Web.Areas.SU_Area.Controllers;
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class TestApiController
    {
        private readonly UnitOfWork _unitOfWork;
        //public readonly IOwinContext _owincontext;
       // private readonly IDataContextAsync dbcontext = new ApplicationDbContext();
        private readonly IDataContextAsync _dbcontext;
        private readonly IMenuForWeekService _weekmenuService;
        private readonly IRepositoryAsync<MenuForWeek> _weekMenuRepository;
        private readonly IMenuForWeekService _menuForWeekService;

        public TestApiController()
        {
            //_dbcontext = new DataContext();
            //_owincontext = new OwinContext();
            _unitOfWork = new UnitOfWork();
            _weekMenuRepository = _unitOfWork.RepositoryAsync<MenuForWeek>();
            _weekmenuService = new MenuForWeekService(_weekMenuRepository);
        }

        [TestMethod]
        public void WeekMenuTestApi()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_weekmenuService);
            WeekMenuDto wmDto = weekMenuApi.GetWeekMenu(12, 2016).Result;
            Assert.IsNotNull(wmDto);
        }

    }
}
