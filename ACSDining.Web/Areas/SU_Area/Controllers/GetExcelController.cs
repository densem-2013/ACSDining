using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.WebPages;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Services;
using Microsoft.Ajax.Utilities;
using Microsoft.SqlServer.Management.Smo;

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

        //[System.Web.Http.HttpPut]
        //[System.Web.Http.Route("paiments")]
        //public HttpResponseMessage GetExelFromWeekPaimentstDto([FromBody] WeekYearDto wyDto)
        //{
        //    if (wyDto == null)
        //    {
        //        wyDto = YearWeekHelp.GetCurrentWeekYearDto();
        //    }
        //    WeekPaimentDto dto = WeekPaimentDto.GetMapDto(_unitOfWork, wyDto);

        //    FileStream fstream = _getExcelService.GetExcelFileFromPaimentsModel(dto).Result;
        //    //string filename = _getExcelService.GetExcelFileFromPaimentsModel(dto);

        //    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        //    {
        //        Content = new StreamContent(fstream)//filename, FileMode.Open, FileAccess.Read), )
        //    };

        //    string _path = HostingEnvironment.MapPath("~/App_Data/DBinitial/Paiments.xlsx");
        //    response.Content.Headers.ContentDisposition =
        //        new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = _path/*"Paiments.xlsx" filename*/};

        //    return response;
        //}
        [System.Web.Http.HttpPut]
        [System.Web.Http.Route("paiments")]
        public FilePathResult GetExelFromWeekPaimentstDto([FromBody] ForExcelDataDto feDto)
        {
            WeekPaimentDto dto = WeekPaimentDto.GetMapDto(_unitOfWork.RepositoryAsync<WeekPaiment>(), feDto.WeekYear);
            string _path = "/ExcelFiles/Paiments.xlsx";
            string filename = _getExcelService.GetExcelFileFromPaimentsModel(feDto);

            return new FilePathResult(_path, "multipart/form-data");
        }

        [System.Web.Http.HttpPut]
        [System.Web.Http.Route("factorders")]
        public FilePathResult GetExelFromWeekOrdersDto([FromBody] ForExcelDataDto feDto)
        {
            WeekPaimentDto dto = WeekPaimentDto.GetMapDto(_unitOfWork.RepositoryAsync<WeekPaiment>(), feDto.WeekYear);
            string _path = "/ExcelFiles/Orders.xlsx";
            try
            {
                string filename = _getExcelService.GetExcelFileFromOrdersModel(feDto);

            }
            catch (Exception)
            {
                    
                throw;
            }

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
