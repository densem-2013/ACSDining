using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Dishes")]
    public class DishesController : ApiController
    {
        private readonly IDishService _dishService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public DishesController(IUnitOfWorkAsync unitOfWork, IDishService dishService)
        {
           // IRepositoryAsync<Dish> _dishRepo = unitOfWork.RepositoryAsync<Dish>();
            _unitOfWork = unitOfWork;
            _dishService = dishService;
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

            List<DishModelDto> dmodels = _dishService.GetDishModelDtoByCategory(category);

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
            try
            {
                _dishService.UpdateDishByDishModel(dish);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DishExists(dish.DishID))
                {
                    return NotFound();
                }
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Dishes
        [HttpPost]
        [Route("create")]
        [ResponseType(typeof(Dish))]
        public async Task<IHttpActionResult> PostDish(DishModelDto dmodel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _dishService.InsertDishByDishModel(dmodel);
            }
            catch (DbUpdateException)
            {
                if (DishExists(dmodel.DishID))
                {
                    return Conflict();
                }
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE api/Dishes/5
        [Route("delete/{id}")]
        [ResponseType(typeof(Dish))]
        public async Task<bool> DeleteDish(int id)
        {

            return await _dishService.DeleteDishById(id);
        }

        private bool DishExists(int id)
        {
            return _dishService.AllDish().Any(e => e.DishID == id);
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