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
        public FilePathResult GetExelFromWeekPaimentstDto([FromBody] WeekYearDto wyDto)
        {
            WeekPaimentDto dto = WeekPaimentDto.GetMapDto(_unitOfWork, wyDto);
            string _path = "/ExcelFiles/Paiments.xlsx";
            string filename = _getExcelService.GetExcelFileFromPaimentsModel(dto);

            return new FilePathResult(_path, "multipart/form-data");
        }

        //[System.Web.Http.HttpPut]
        //[System.Web.Http.Route("paiments")]
        //public HttpResponseMessage GetExelFromWeekPaimentstDto([FromBody] WeekYearDto wyDto)
        //{
        //    HttpResponseMessage result = null;
        //    var localFilePath = HttpContext.Current.Server.MapPath("~/ExcelFiles/Paiments.xlsx");
        //    //string localFilePath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
        //    //                                 @"ACSDining.Web\App_Data\DBinitial\Paiments.xls";
        //    WeekPaimentDto dto = WeekPaimentDto.GetMapDto(_unitOfWork, wyDto);
        //    //FileStream fstream = _getExcelService.GetExcelFileFromPaimentsModel(dto).Result;
        //    string _path = _getExcelService.GetExcelFileFromPaimentsModel(dto);
        //    FileStream fs = new FileStream(localFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        //    // check if parameter is valid
        //    if (String.IsNullOrEmpty(localFilePath))
        //    {
        //        result = Request.CreateResponse(HttpStatusCode.BadRequest);
        //    }
        //    // check if file exists on the server
        //    //else if (String.IsNullOrEmpty(_path))
        //    //{
        //    //    result = Request.CreateResponse(HttpStatusCode.Gone);
        //    //}
        //    else
        //    {// serve the file to the client
        //        result = Request.CreateResponse(HttpStatusCode.OK);
        //        result.Content = new StreamContent(fs);
        //        result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        //        {
        //            FileName = "Paiments.xlsx"
        //        };
        //        result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/ms-excel");
        //        result.Content.Headers.ContentLength = fs.Length;
        //    }

        //    return result;
        //}

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
