using System.Collections.Generic;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Service
{
    public interface IDishService: IService<Dish>
    {
        List<DishModelDto> GetAllDishModelDto();
        List<DishModelDto> GetDishModelDtoByCategory(string category);
        void UpdateDishByDishModel(DishModelDto dmodel);
        void InsertDishByDishModel(DishModelDto dmodel);
        List<Dish> AllDish();
        Dish GetDishById(int id);
        Task<bool> DeleteDishById(int id);
    }
}