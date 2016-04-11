using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Infrastructure.Repositories
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

        public static Dish UpdateDish(this IRepositoryAsync<Dish> repository, DishModelDto dmodel)
        {
            Dish target = repository.Find(dmodel.DishID);
            if (!string.Equals(dmodel.Foods, string.Empty))
            {
                DishDetail dishDetail = new DishDetail {Foods = dmodel.Foods};
                target.DishDetail.Foods = dmodel.Foods;
                target.DishDetail = dishDetail;
            }
            target.Price = dmodel.Price;
            target.Title = dmodel.Title;

            return target;
        }

    }
}
