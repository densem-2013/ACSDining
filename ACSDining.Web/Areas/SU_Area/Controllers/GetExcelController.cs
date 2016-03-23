using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [System.Web.Http.Authorize(Roles = "SuperUser")]
    [System.Web.Http.RoutePrefix("api/GetExcel")]
    public class GetExcelController : ApiController
    {

        private readonly IGetExcelService _getExcelService;

        public GetExcelController(IGetExcelService getExcelService)
        {
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
          
    }
}
