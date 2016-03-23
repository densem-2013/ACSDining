using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Repository.Repositories
{
    public static class DishRepository
    {
        public static List<DishModelDto> AllDishModelDto(this IRepositoryAsync<Dish> repository)
        {
            return repository.Queryable().Select(DishModelDto.MapDto).ToList();
        }

        public static List<DishModelDto> DishModelDtoByCategory(this IRepositoryAsync<Dish> repository, string category)
        {
            return
                repository.Queryable()
                    .Where(d => string.Equals(d.DishType.Category, category))
                    .Select(DishModelDto.MapDto)
                    .ToList();
        }

        public static void UpdateDish(this IRepositoryAsync<Dish> repository, DishModelDto dmodel)
        {
            Dish target = repository.Find(dmodel.DishID);
            if (!string.Equals(dmodel.Foods, string.Empty))
            {
                DishDetail dishDetail = new DishDetail {Foods = dmodel.Foods};
                repository.GetRepository<DishDetail>().Insert(dishDetail);
                target.DishDetail.Foods = dmodel.Foods;
                target.DishDetail = dishDetail;
            }
            target.Price = dmodel.Price;
            target.Title = dmodel.Title;
            repository.Update(target);
        }

        public static void InsertDish(this IRepositoryAsync<Dish> repository, DishModelDto dmodel)
        {
            Dish newdish = new Dish
            {
                Title = dmodel.Title,
                Price = dmodel.Price,
                ProductImage = dmodel.ProductImage,
                DishType =
                    repository.GetRepository<DishType>()
                        .Queryable()
                        .FirstOrDefault(dt => string.Equals(dt.Category, dmodel.Category)),
                DishDetail = new DishDetail
                {
                    Foods = dmodel.Foods
                }
            };
            repository.Insert(newdish);
        }
    }
}
