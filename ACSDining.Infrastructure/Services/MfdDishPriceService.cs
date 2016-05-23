using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IMfdDishPriceService
    {
        //Получить блюда по категориям
        List<DishModelDto> GetDishModelDtoByCategory(string category,int? menufordayid=null);
        //Обновить блюдо
        void UpdateDish(DishModelDto dish);
        //Создать блюдо
        void InsertDish(DishModelDto dish);
        //Удалить блюдо
        void DeleteDish(int dishid);
        //Преобразовать блюдо в DishModelDto
        DishModelDto GetDishModelDto(Dish dish, int menufordayid);
        //Обновить дневное меню
        void UpdateMenuForDay(MenuForDayDto dayMenuDto);
    }
    public class MfdDishPriceService : Service<MfdDishPriceRelations>, IMfdDishPriceService
    {
        private readonly IRepositoryAsync<MfdDishPriceRelations> _repository;

        public MfdDishPriceService(IRepositoryAsync<MfdDishPriceRelations> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public List<DishModelDto> GetDishModelDtoByCategory(string category, int? menufordayid = null)
        {
            return _repository.DishesByCategory(category, menufordayid);
        }

        public DishModelDto GetDishModelDto(Dish dish, int menufordayid)
        {
            return _repository.GetDishModelDto(dish, menufordayid);
        }

        public void UpdateDish(DishModelDto dish)
        {
            _repository.UpdateDish(dish);
        }
        public void DeleteDish(int dishid)
        {
            _repository.DeleteDish(dishid);
        }

        public void InsertDish(DishModelDto dish)
        {
            _repository.InsertDish(dish);
        }

        public void UpdateMenuForDay(MenuForDayDto dayMenuDto)
        {
            _repository.UpdateMfdDishes(dayMenuDto);
        }
    }
}
