using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using ACSDining.Core.UnitOfWork;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [System.Web.Http.Authorize(Roles = "SuperUser")]
    [System.Web.Http.RoutePrefix("api/GetExcel")]
    public class GetExcelController : ApiController
    {

        private readonly IGetExcelService _getExcelService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public GetExcelController(IUnitOfWorkAsync unitOfWork,IGetExcelService getExcelService)
        {
            _unitOfWork = unitOfWork;
            _getExcelService = getExcelService;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("paiments")]
        [ResponseType(typeof(double))]
        public FilePathResult GetExelFromPaimentDto([FromBody] PaimentsDTO paimodel)
        {
            string filename = _getExcelService.PaimentsDtoToExcelFile(paimodel);
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
