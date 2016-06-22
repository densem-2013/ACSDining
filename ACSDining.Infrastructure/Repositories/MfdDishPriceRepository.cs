using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Identity;
using DishModelDto = ACSDining.Infrastructure.DTO.SuperUser.Dishes.DishModelDto;
using MenuForDayDto = ACSDining.Infrastructure.DTO.SuperUser.Menu.MenuForDayDto;
using WeekMenuDto = ACSDining.Infrastructure.DTO.SuperUser.Menu.WeekMenuDto;

namespace ACSDining.Infrastructure.Repositories
{
    public static class MfdDishPriceRepository
    {
        public static List<DishModelDto> DishesByCategory(this IRepositoryAsync<MfdDishPriceRelations> repository,
            string category, int? menufordayid = null)
        {
            return
                repository.GetRepositoryAsync<Dish>().Query()
                    .Include(d => d.DishDetail)
                    .Include(d => d.DishType)
                    .Include(d => d.CurrentPrice)
                    .Include(d => d.MfdDishPriceRelations)
                    .Select()
                    .Where(d => string.Equals(d.DishType.Category, category))
                    .Select(d => repository.GetDishModelDto(d, menufordayid))
                    .ToList();
        }

        public static DishModelDto GetDishModelDto(this IRepositoryAsync<MfdDishPriceRelations> repository, Dish dish,
            int? menufordayid = null)
        {
            DishModelDto dto = null;
            double price = 0.00;
            if (menufordayid == null)
            {
                price = dish.CurrentPrice.Price;
            }
            else
            {
                MfdDishPriceRelations first =
                    repository.Query()
                        .Include(mp => mp.DishPrice)
                        .Select()
                        .FirstOrDefault(mfdpr => mfdpr.DishId == dish.DishID && mfdpr.MenuForDayId == menufordayid);
                if (first != null)
                {
                    price = first.DishPrice.Price;
                }
            }
            dto = new DishModelDto
            {
                DishId = dish.DishID,
                Title = dish.Title /*?? "Блюдо не выбрано"*/,
                ProductImage = dish.ProductImage,
                Price = price,
                Category = dish.DishType.Category,
                Description = dish.Description,
                Deleted = dish.Deleted
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
                    .Where(d => mfdrels.Select(md => md.DishId).Contains(d.DishID)).OrderBy(d => d.DishType.Id)
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
                MfdModels = mfw.MenuForDay.Select(repository.GetMenuForDayDto).ToList(),
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
                repository.Context.Entry(dishPrice).State = EntityState.Added;
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

        public static void DeleteDish(this IRepositoryAsync<MfdDishPriceRelations> repository, int dishid)
        {
            Dish updish = repository.GetRepositoryAsync<Dish>().Find(dishid);
            updish.Deleted = true;
            repository.Context.Entry(updish).State = EntityState.Modified;
        }

        public static void InsertDish(this IRepositoryAsync<MfdDishPriceRelations> repository, DishModelDto dishmodel)
        {
            Dish newdish = new Dish
            {
                Title = dishmodel.Title,
                CurrentPrice = repository.GetDishPrice(dishmodel.Price),
                Description = dishmodel.Description,
                ProductImage = dishmodel.ProductImage,
                DishType =
                    repository.GetRepositoryAsync<DishType>()
                        .Queryable()
                        .FirstOrDefault(dt => string.Equals(dt.Category, dishmodel.Category))
            };
            repository.Context.Entry(newdish).State = EntityState.Added;
        }

        public static int UpdateMfdDishes(this IRepositoryAsync<MfdDishPriceRelations> repository,
            MenuForDayDto menuforday)
        {

            MenuForDay menuFd =
                repository.GetRepositoryAsync<MenuForDay>()
                    .Query()
                    .Include(mfd => mfd.DishPriceMfdRelations.Select(dp => dp.DishPrice))
                    .Include(mfd => mfd.DishPriceMfdRelations.Select(dp => dp.Dish))
                    .Include(mfd => mfd.DishPriceMfdRelations.Select(dp => dp.Dish.DishType))
                    .Include(mfd => mfd.DayOrderMenus.Select(dp => dp.MenuForDay))
                    .Include(mfd => mfd.DayOrderMenus.Select(dp => dp.MenuForDay.WorkingDay.DayOfWeek))
                    .Include(mfd => mfd.DayOrderMenus.Select(dp => dp.WeekOrderMenu.DayOrderMenus))
                    .Include(mfd => mfd.WorkingDay.WorkingWeek.Year)
                    .Include(mfd => mfd.DayOrderMenus.Select(dord => dord.WeekOrderMenu.MenuForWeek))
                    .Select()
                    .FirstOrDefault(mfd => mfd.ID == menuforday.Id);

            if (menuFd != null)
            {
                int changedishid = menuforday.Dishes.Select(d => d.DishId)
                        .FirstOrDefault(id => ! menuFd.DishPriceMfdRelations.Select(dp => dp.DishId).Contains(id));
                if (changedishid==0)
                {
                    for (int i = 0; i < menuFd.DishPriceMfdRelations.Count; i++)
                    {
                        if (menuFd.DishPriceMfdRelations.ElementAt(i).DishPrice!=repository.GetDishPrice(menuforday.Dishes.ElementAt(i).Price))
                        {
                            changedishid = menuFd.DishPriceMfdRelations.ElementAt(i).DishId;
                        }
                    }
                }

                menuFd.DishPriceMfdRelations = repository.MfdByMenuForDayDto(menuforday);
                Dish dish =
                    repository.GetRepositoryAsync<Dish>()
                        .Query()
                        .Include(d => d.DishType)
                        .Include(d => d.CurrentPrice)
                        .Select()
                        .FirstOrDefault(d => d.DishID == changedishid);
                MfdDishPriceRelations rel =
                    menuFd.DishPriceMfdRelations.FirstOrDefault(
                        mfd => dish != null && string.Equals(mfd.Dish.DishType.Category, dish.DishType.Category));

                if (rel != null && dish != null)
                {
                    rel.DishId = changedishid;
                    var firstOrDefault = menuforday.Dishes.FirstOrDefault(d => d.DishId == changedishid);
                    if (firstOrDefault != null)
                        dish.CurrentPrice = repository.GetDishPrice(firstOrDefault.Price);
                    rel.DishPrice = dish.CurrentPrice;
                    repository.Context.Entry(rel).State = EntityState.Modified;
                    repository.Context.Entry(dish).State = EntityState.Modified;

                    menuFd.TotalPrice = menuforday.TotalPrice;

                    repository.Context.Entry(menuFd).State = EntityState.Modified;
                    
                    return dish.DishType.Id;
                }
            }
            return 0;
        }

        public static List<MfdDishPriceRelations> MfdByMenuForDayDto(
            this IRepositoryAsync<MfdDishPriceRelations> repository, MenuForDayDto menuforday)
        {
            return repository.Query()
                .Include(mfd => mfd.DishPrice)
                .Include(mfd => mfd.MenuForDay.WorkingDay.WorkingWeek)
                .Include(mfd => mfd.MenuForDay.DayOrderMenus.Select(dom => dom.WeekOrderMenu.DayOrderMenus))
                .Include(mfd => mfd.MenuForDay.DayOrderMenus.Select(dom => dom.MenuForDay.DishPriceMfdRelations))
                .Select()
                .Where(mdp => mdp.MenuForDayId == menuforday.Id)
                .ToList();
        }
    }
}
