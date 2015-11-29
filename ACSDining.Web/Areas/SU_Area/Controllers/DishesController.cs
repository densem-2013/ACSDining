using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;
using ACSDining.Web.Areas.SU_Area.Models;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/Dishes")]
    public class DishesController : ApiController
    {
        private IUnitOfWork unitOfWork;
        private IRepository<Dish> _dishRepository;
        private IRepository<DishType> _dishtypeRepository;
        //private ApplicationDbContext db ;

        List<DishModel> Dishes
        {
            get
            {
                return _dishRepository.GetAll().Select(d => new DishModel()
                {
                    DishID = d.DishID,
                    Title = d.Title,
                    ProductImage = d.ProductImage,
                    Price = d.Price,
                    Category = d.DishType.Category,
                    Foods = d.DishDetail.Foods

                }).ToList();
            }
        }

        public DishesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            _dishRepository = this.unitOfWork.Repository<Dish>();

        }


        [HttpGet]
        [Route("byCategory/{category}")]
        [ResponseType(typeof(IEnumerable<DishModel>))]
        // GET api/Dishes
        public async Task<IHttpActionResult> GetByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return NotFound();
            }

            List<DishModel> dmodels = Dishes.Where(d => string.Equals(d.Category, category)).ToList();

            return Ok(dmodels);
        }


        // PUT api/Dishes/5
        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> PutDish([FromBody] DishModel dish)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Dish target = _dishRepository.Find(d=>d.DishID==dish.DishID);
                target.DishDetail.Foods = dish.Foods;
                target.Price = dish.Price;
                target.Title = dish.Title;
                _dishRepository.Update(target);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DishExists(dish.DishID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Dishes
        [HttpPost]
        [Route("create")]
        [ResponseType(typeof(Dish))]
        public async Task<IHttpActionResult> PostDish(DishModel dmodel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Dish newdish = new Dish
            {
                Title = dmodel.Title,
                Price = dmodel.Price,
                ProductImage = dmodel.ProductImage,
                DishType = _dishtypeRepository.GetAll().AsEnumerable().FirstOrDefault(dt => string.Equals(dt.Category, dmodel.Category)),
                DishDetail = new DishDetail
                {
                    Foods = dmodel.Foods
                }
            };

            try
            {
                _dishRepository.Insert(newdish);
            }
            catch (DbUpdateException)
            {
                if (DishExists(newdish.DishID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            //return CreatedAtRoute("DefaultApi", new { id = dish.DishID }, dish); 
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE api/Dishes/5
        [Route("delete/{id}")]
        [ResponseType(typeof(Dish))]
        public async Task<IHttpActionResult> DeleteDish(int id)
        {
            Dish dish = _dishRepository.GetById(id);
            if (dish == null)
            {
                return NotFound();
            }

            _dishRepository.Delete(dish);

            return Ok();
        }

        private bool DishExists(int id)
        {
            return _dishRepository.GetAll().Count(e => e.DishID == id) > 0;
        }
    }
}