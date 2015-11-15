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

        List<DishModel> Dishes { get; set; }

        public DishesController()
        {
            db = new ApplicationDbContext();

            LoadDishes();

        }
        private void LoadDishes()
        {
            Dishes = db.Dishes.Select(d => new DishModel()
            {
                DishID = d.DishID,
                Title = d.Title,
                ProductImage = d.ProductImage,
                Price = d.Price,
                Category = d.DishType.Category,
                Foods = d.DishDetail.Foods

            }).ToList();
        }
        // GET api/Dishes
        public List<DishModel> GetDishes()
        {
            return Dishes;
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

        // GET api/Dishes/5
        [ResponseType(typeof(Dish))]
        public async Task<IHttpActionResult> GetDish(int id)
        {
            Dish dish = await db.Dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }

            return Ok(dish);
        }

        // PUT api/Dishes/5
        public async Task<IHttpActionResult> PutDish(int id, Dish dish)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dish.DishID)
            {
                return BadRequest();
            }

            db.Entry(dish).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DishExists(id))
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
        [ResponseType(typeof(Dish))]
        public async Task<IHttpActionResult> PostDish(Dish dish)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Dishes.Add(dish);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DishExists(dish.DishID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = dish.DishID }, dish);
        }

        // DELETE api/Dishes/5
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