using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Services;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [System.Web.Http.Authorize(Roles = "SuperUser")]
    [System.Web.Http.RoutePrefix("api/GetExcel")]
    public class GetExcelController : ApiController
    {

        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IGetExcelService _getExcelService;
        private readonly IMenuForWeekService _menuForWeekService;

        public GetExcelController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _getExcelService = new GetExcelService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            _menuForWeekService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
        }


        [System.Web.Http.HttpPut]
        [System.Web.Http.Route("paiments")]
        [ResponseType(typeof(double))]
        public FilePathResult GetExelFromWeekPaimentstDto([FromBody] List<UserWeekPaimentDto> paimentList)
        {
            UserWeekPaimentDto first = paimentList.FirstOrDefault();
            WeekYearDto wyDto = first != null ? first.WeekYear : null;
            WorkingWeek workweek = _menuForWeekService.GetWorkWeekByWeekYear(wyDto);

            string[] dishCategories = MapHelper.GetCategoriesStrings(_unitOfWork);

            string filename = _getExcelService.GetExcelFileFromPaimentsModel(paimentList, dishCategories, workweek);

            return new FilePathResult(filename, "multipart/form-data");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
