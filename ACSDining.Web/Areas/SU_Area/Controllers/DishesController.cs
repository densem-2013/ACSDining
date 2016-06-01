using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.DTO.SuperUser.Dishes;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Dishes")]
    public class DishesController : ApiController
    {
        private readonly IMfdDishPriceService _mfdDishPriceService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public DishesController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _mfdDishPriceService = new MfdDishPriceService(_unitOfWork.RepositoryAsync<MfdDishPriceRelations>());
        }


        [HttpGet]
        [Route("byCategory/{category}")]
        [ResponseType(typeof(IEnumerable<DishModelDto>))]
        // GET api/Dishes
        public async Task<IHttpActionResult> GetByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return NotFound();
            }

            List<DishModelDto> dmodels = _mfdDishPriceService.GetDishModelDtoByCategory(category);

            return Ok(dmodels);
        }


        // PUT api/Dishes/5
        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> PutDish([FromBody] DishModelDto dish)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _mfdDishPriceService.UpdateDish(dish);

            await _unitOfWork.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Dishes
        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> PostDish(DishModelDto dmodel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _mfdDishPriceService.InsertDish(dmodel);

            await  _unitOfWork.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE api/Dishes/5
        [HttpPut]
        [Route("updateDeleted")]
        public async Task<IHttpActionResult> DeleteDish([FromBody]UpdateDishDeleted updel)
        {
            ApplicationDbContext db = _unitOfWork.GetContext();
            Dish delDish = db.Dishes.Find(updel.DishId);
            if (delDish == null)
            {
                return NotFound();
            }
            delDish.Deleted = updel.Deleted;

            db.Entry(delDish).State = EntityState.Modified;

            await _unitOfWork.SaveChangesAsync();

            return Ok(delDish.Deleted);
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