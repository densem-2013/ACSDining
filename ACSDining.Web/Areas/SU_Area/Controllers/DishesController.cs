﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Dishes")]
    public class DishesController : ApiController
    {
        private ApplicationDbContext _db;
        private readonly IMfdDishPriceService _mfdDishPriceService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public DishesController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _db = _unitOfWork.GetContext();
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
            try
            {
               //Dish updish= _dishService.UpdateDishByDishModel(dish);
                Dish updish = _db.Dishes.Find(dish.DishId);
                updish.Description = dish.Description;
                updish.Price = dish.Price;
                updish.Title = dish.Title;
                //_db.Dishes.Remove(updish);
                //_db.Dishes.Add(updish);
                _db.Entry(updish).State=EntityState.Modified;
                _db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DishExists(dish.DishId))
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
                Dish newdish = new Dish
                {
                    Title = dmodel.Title,
                    Price = dmodel.Price,
                    Description = dmodel.Description,
                    ProductImage = dmodel.ProductImage,
                    DishType =MapHelper.GetDishCategories(_unitOfWork).Find(dt => string.Equals(dt.Category, dmodel.Category))//,
                    //DishDetail = new DishDetail
                    //{
                    //    Foods = dmodel.Foods
                    //}
                };

                _db.Dishes.Add(newdish);
               // _dishService.Insert(newdish);
               await  _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DishExists(dmodel.DishId))
                {
                    return Conflict();
                }
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE api/Dishes/5
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IHttpActionResult> DeleteDish(int id)
        {
            Dish dish = _dishService.Find(id);
            _db.Dishes.Remove(dish);
            await _unitOfWork.SaveChangesAsync();
            return Ok(true);
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