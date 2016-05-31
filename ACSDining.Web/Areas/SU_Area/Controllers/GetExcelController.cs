using System;
using System.Web.Http;
using System.Web.Mvc;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;
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
        public FilePathResult GetExelFromWeekPaimentstDto([FromBody] ForExcelDataDto feDto)
        {
            WeekPaimentDto dto = WeekPaimentDto.GetMapDto(_unitOfWork.RepositoryAsync<WeekPaiment>(), feDto.WeekYear);
            string filename = _getExcelService.GetExcelFileFromPaimentsModel(feDto);
            string _path = string.Format("/ExcelFiles/{0}",
                filename.Substring(filename.LastIndexOf(@"\", StringComparison.Ordinal) + 1));
            return new FilePathResult(_path, "multipart/form-data");
        }

        [System.Web.Http.HttpPut]
        [System.Web.Http.Route("orders")]
        public FilePathResult GetExelFromWeekOrdersDto([FromBody] ForExcelDataDto feDto)
        {
            WeekPaimentDto dto = WeekPaimentDto.GetMapDto(_unitOfWork.RepositoryAsync<WeekPaiment>(), feDto.WeekYear);
            string filename = string.Equals(feDto.ItsFact, "fact")
                ? _getExcelService.GetExcelFileFromOrdersModel(feDto)
                : _getExcelService.GetExcelFromPlanOrdersModel(feDto);
                string _path = string.Format("/ExcelFiles/{0}",
                    filename.Substring(filename.LastIndexOf(@"\", StringComparison.Ordinal) + 1));

            return new FilePathResult(_path, "multipart/form-data");
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
