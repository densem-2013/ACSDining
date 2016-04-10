using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
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
        public FilePathResult GetExelFromPaimentDto([FromBody] PaimentsDto paimodel)
        {
            WorkingWeek workweek = _workDaysService.GetWorkWeekByWeekYear(paimodel.WeekNumber, paimodel.YearNumber);
            List<DishType> dtypes = _unitOfWork.RepositoryAsync<DishType>().Queryable().ToList();

            string filename = _getExcelService.PaimentsDtoToExcelFile(paimodel, workweek,dtypes);

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
