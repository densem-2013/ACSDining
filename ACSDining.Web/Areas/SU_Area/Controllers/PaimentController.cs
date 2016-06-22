using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser.Paiments;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Paiment")]
    public class PaimentController : ApiController
    {
        //private readonly ApplicationDbContext _db;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IWeekPaimentService _weekPaimentService;

        public PaimentController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            //_db = unitOfWorkAsync.GetContext();
            _weekPaimentService = new WeekPaimentService(_unitOfWork.RepositoryAsync<WeekPaiment>());
        }

        /// <summary>
        /// Получить все оплаты за неделю
        /// </summary>
        /// <param name="wyDto">Объект, инкапсулирующий запрашиваемую неделю в году</param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        [ResponseType(typeof (WeekPaimentDto))]
        public async Task<WeekPaimentDto> GetWeekPaiments([FromBody] WeekYearDto wyDto)
        {
            if (wyDto == null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }

            WeekPaimentDto dto = WeekPaimentDto.GetMapDto(_unitOfWork.RepositoryAsync<WeekPaiment>(), wyDto);

            return await  Task.FromResult(dto);
        }

       

        [HttpPut]
        [Route("updatePaiment")]
        [ResponseType(typeof(double))]
        public async Task<IHttpActionResult> UpdatePaiment([FromBody]UpdateWeekPaimentDto upwpDto)
        {
            if (upwpDto == null)
            {
                return BadRequest();
            }
            _unitOfWork.GetContext().UpdateWeekPaiment(upwpDto);

            return Ok(true);
        }

        [HttpPut]
        [Route("updateNote")]
        [ResponseType(typeof (double))]
        public async Task<IHttpActionResult> UpdateNote([FromBody] NoteUpdateDto upNoteDto)
        {
            if (upNoteDto == null)
            {
                return BadRequest();
            }
            ApplicationDbContext db = _unitOfWork.GetContext();
            WeekPaiment wpai = db.WeekPaiments.Include("WeekOrderMenu").FirstOrDefault(wp=>wp.Id==upNoteDto.Id);
            if (wpai != null)
            {
                wpai.Note = upNoteDto.Note;
                db.Entry(wpai).State = EntityState.Modified;
                await _unitOfWork.SaveChangesAsync();
            }

            return Ok(true);
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
