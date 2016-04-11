using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using ACSDining.Core.Domains;
using ACSDining.Core.DTO;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [System.Web.Http.Authorize(Roles = "SuperUser")]
    [System.Web.Http.RoutePrefix("api/GetExcel")]
    public class GetExcelController : ApiController
    {

        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IGetExcelService _getExcelService;
        private readonly IWorkDaysService _workDaysService;

        public GetExcelController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _getExcelService = new GetExcelService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            _workDaysService=new WorkDaysService(_unitOfWork.RepositoryAsync<WorkingWeek>());
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("paiments")]
        [ResponseType(typeof(double))]
        public FilePathResult GetExelFromWeekPaimentstDto([FromBody] List<UserWeekPaimentDto> paimentList)
        {
            UserWeekPaimentDto first = paimentList.FirstOrDefault();
            WeekYearDto wyDto = first != null ? first.WeekYear : null;
            WorkingWeek workweek = _workDaysService.GetWorkWeekByWeekYear(wyDto);

            string[] dishCategories = MapHelper.GetCategories(_unitOfWork);

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
