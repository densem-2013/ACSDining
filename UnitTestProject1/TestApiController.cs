////using ACSDining.Core.DataContext;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DAL;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Infrastructure.Identity;
using ACSDining.Service;
using ACSDining.Web.Areas.SU_Area.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class TestApiController
    {
        private readonly UnitOfWork _unitOfWork;
        //public readonly IOwinContext _owincontext;
        // private readonly IDataContextAsync dbcontext = new ApplicationDbContext();
        //private readonly IDataContextAsync _dbcontext;
        private readonly IMenuforDayService _menuForDayService;
        private readonly IRepositoryAsync<MenuForWeek> _weekMenuRepository;
        private readonly IMenuForWeekService _menuForWeekService;
        private readonly IDishService _dishService;

        public TestApiController()
        {
            //_dbcontext = new DataContext();
            //_owincontext = new OwinContext();
            _unitOfWork = new UnitOfWork();
            _weekMenuRepository = _unitOfWork.RepositoryAsync<MenuForWeek>();
            _menuForWeekService = new MenuForWeekService(_weekMenuRepository);
            IRepositoryAsync<MenuForDay> mfdrepo = _unitOfWork.RepositoryAsync<MenuForDay>();
            _menuForDayService = new MenuForDayService(mfdrepo);
            IRepositoryAsync<Dish> _dishRepo = _unitOfWork.RepositoryAsync<Dish>();
            _dishService = new DishService(_dishRepo);
        }

        [TestMethod]
        public  void WeekMenuTestApi()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork, _menuForWeekService, _dishService,
                _menuForDayService);
            WeekMenuDto wmDto = weekMenuApi.GetWeekMenu(12, 2016).Result;
            Assert.IsNotNull(wmDto);
        }

    }
}
