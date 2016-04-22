﻿using System.Threading.Tasks;
using System.Web.Http;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "Employee,SuperUser")]
    [RoutePrefix("api/WorkDays")]
    public class WorkDayApiController : ApiController
    {
        private readonly ApplicationDbContext _db;
        private readonly IWorkDaysService _workDayService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public WorkDayApiController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _db = unitOfWorkAsync.GetContext();
            _workDayService = new WorkDaysService(_unitOfWork.RepositoryAsync<WorkingWeek>());
        }

        [HttpPut]
        [Route("")]
        public async Task<WorkWeekDto> GetWorkWeek([FromBody] WeekYearDto wyDto)
        {
            if (wyDto==null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }
            return
                await
                    Task.FromResult(_workDayService.GetWorkWeekDtoByWeekYear(wyDto));
        }

        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateWorkDays(WorkWeekDto weekModel)
        {
            WorkingWeek workweek = _workDayService.Find(weekModel.WorkWeekId);
            if (workweek == null)
            {
                return NotFound();
            }


            WorkingWeek wweek=_workDayService.UpdateWorkDays(weekModel);

            _db.WorkingWeeks.Remove(wweek);
            _db.WorkingWeeks.Add(wweek);

            await _unitOfWork.SaveChangesAsync();

            return Ok();
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
