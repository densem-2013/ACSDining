using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Web.Areas.SU_Area.Models;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/Dishes")]
    public class DishesController : ApiController
    {
        private ApplicationDbContext db ;

        List<DishModel> Dishes
        {
            get
            {
                return db.Dishes.Select(d => new DishModel()
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

        public DishesController()
        {
            db = new ApplicationDbContext();

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
                Dish target = db.Dishes.Find(dish.DishID);
                target.DishDetail.Foods = dish.Foods;
                target.Price = dish.Price;
                target.Title = dish.Title;
                db.Entry(target).State = EntityState.Modified;
                await db.SaveChangesAsync();
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
                DishType = db.DishTypes.AsEnumerable().FirstOrDefault(dt => string.Equals(dt.Category, dmodel.Category)),
                DishDetail = new DishDetail
                {
                    Foods = dmodel.Foods
                }
            };

            db.Dishes.Add(newdish);

            try
            {
                await db.SaveChangesAsync();
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
            Dish dish = await db.Dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }

            db.Dishes.Remove(dish);
            await db.SaveChangesAsync();

            return Ok(dish);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DishExists(int id)
        {
            return db.Dishes.Count(e => e.DishID == id) > 0;
        }
    }
}