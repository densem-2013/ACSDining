using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.Repositories
{
    public static class MfdDishPriceRepository
    {
        public static List<DishModelDto> DishesByCategory(this IRepositoryAsync<MfdDishPriceRelations> repository,
            string category, int? menufordayid=null)
        {
            if (menufordayid==null)
            {
                menufordayid = repository.GetRepositoryAsync<MenuForWeek>().GetCurrentMenuForDay().ID;
            }
            return
                repository.GetRepositoryAsync<Dish>()
                    .DishesByCategory(category)
                    .Select(d => repository.GetDishModelDto(d, menufordayid.Value))
                    .ToList();
        }

        public static DishModelDto GetDishModelDto(this IRepositoryAsync<MfdDishPriceRelations> repository, Dish dish, int menufordayid)
        {
            DishModelDto dto = null;
            MfdDishPriceRelations first = repository.Query().Include(mp => mp.DishPrice).Select().FirstOrDefault(mfdpr => mfdpr.DishId == dish.DishID && mfdpr.MenuForDayId == menufordayid);
            double price = 0.00;
            if (first != null)
            {
                price = first.DishPrice.Price;
            }
            dto = new DishModelDto
            {
                DishId=dish.DishID,
                Title=dish.Title,
                ProductImage=dish.ProductImage,
                Price=price,
                Category=/*dish.DishType!=null?*/dish.DishType.Category,//:"",
                Description =dish.Description
            };
            return dto;
        }

        public static MenuForDayDto GetMenuForDayDto(this IRepositoryAsync<MfdDishPriceRelations> repository,
            MenuForDay mfd)
        {
            List<MfdDishPriceRelations> mfdrels = repository.Queryable().Where(mp => mp.MenuForDayId == mfd.ID).ToList();
            List<Dish> dishes =
                repository.GetRepositoryAsync<Dish>()
                    .Queryable().Include("DishType").Include("DishDetail")
                    .Where(d => mfdrels.Select(md => md.DishId).Contains(d.DishID)).OrderBy(d=>d.DishType.Id)
                    .ToList();
            List<DishModelDto> dishmodels = dishes.Select(d => repository.GetDishModelDto(d, mfd.ID)).ToList();
            return new MenuForDayDto
            {
                Id = mfd.ID,
                TotalPrice = mfd.TotalPrice,
                Dishes = dishmodels,
                OrderCanBeChanged = mfd.OrderCanBeChanged,
                OrderWasBooking = repository.GetRepositoryAsync<WeekOrderMenu>().GetUsersMadeOrder(mfd.ID).Any()
            };
        }

        public static WeekMenuDto GetWeekMenuDto(this IRepositoryAsync<MfdDishPriceRelations> repository,
            MenuForWeek mfw)
        {
            WeekYearDto wyDto = WeekYearDto.MapDto(mfw.WorkingWeek);
            return new WeekMenuDto
            {
                Id = mfw.ID,
                SummaryPrice = mfw.SummaryPrice,
                MfdModels =mfw.MenuForDay.Select(repository.GetMenuForDayDto).ToList(),
                OrderCanBeCreated = mfw.OrderCanBeCreated,
                WorkWeekDays = mfw.WorkingWeek.WorkingDays.Select(wd => wd.IsWorking).ToArray(),
                WorkingDaysAreSelected = mfw.WorkingDaysAreSelected,
                DayNames = repository.Context.GetDayNames(wyDto).Result,
                WeekYear = wyDto
            };
        }

        public static DishPrice GetDishPrice(this IRepositoryAsync<MfdDishPriceRelations> repository, double price)
        {
            DishPrice dishPrice =
                repository.GetRepositoryAsync<DishPrice>()
                    .Queryable()
                    .FirstOrDefault(dp => Math.Abs(dp.Price - price) < 0.001);
            if (dishPrice == null)
            {
                dishPrice = new DishPrice {Price = price};
                repository.Context.Entry(dishPrice).State=EntityState.Added;
            }
            return dishPrice;
        }

        public static void UpdateDish(this IRepositoryAsync<MfdDishPriceRelations> repository, DishModelDto dish)
        {
            Dish updish = repository.GetRepositoryAsync<Dish>().Find(dish.DishId);
            updish.Description = dish.Description;
            updish.Title = dish.Title;
            updish.CurrentPrice = repository.GetDishPrice(dish.Price);
            repository.Context.Entry(updish).State = EntityState.Modified;
        }
    }
}
