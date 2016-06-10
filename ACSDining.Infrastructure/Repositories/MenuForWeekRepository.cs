using System;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser.Menu;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.Repositories
{
    public static class MenuForWeekRepository
    {

        public static MenuForWeek GetWeekMenuByWeekYear(this IRepositoryAsync<MenuForWeek> repository, WeekYearDto wyDto)
        {
            MenuForWeek mfw =
                repository.Query()
                    .Include(wm => wm.MenuForDay.Select(dm => dm.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                    .Include(wm => wm.MenuForDay.Select(dm => dm.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                    .Include(wm => wm.MenuForDay.Select(dm => dm.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                    .Include(wm => wm.Orders)
                    .Include(wm => wm.WorkingWeek.Year)
                    .Include(wm => wm.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                    .Select()
                    .FirstOrDefault(
                        wm => wm.WorkingWeek.WeekNumber == wyDto.Week && wm.WorkingWeek.Year.YearNumber == wyDto.Year);

            return mfw;
        }

        public static MenuForWeek GetMenuById(this IRepositoryAsync<MenuForWeek> repository, int menuid)
        {
            MenuForWeek mfw =
                repository.Query()
                    .Include(wm => wm.MenuForDay.Select(dm => dm.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                    .Include(wm => wm.MenuForDay.Select(dm => dm.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                    .Include(wm => wm.MenuForDay.Select(dm => dm.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                    .Include(wm => wm.Orders)
                    .Include(wm => wm.WorkingWeek.Year)
                    .Include(wm => wm.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                    .Select()
                    .FirstOrDefault(wm => wm.ID == menuid);

            return mfw;
        }

        public static WorkingWeek WorkWeekByWeekYear(this IRepositoryAsync<MenuForWeek> repository, WeekYearDto wyDto)
        {
            return repository.GetRepositoryAsync<WorkingWeek>().Query()
                .Include(ww => ww.WorkingDays.Select(wd => wd.DayOfWeek))
                .Include(ww => ww.Year)
                .Select()
                .FirstOrDefault(ww => ww.WeekNumber == wyDto.Week && ww.Year.YearNumber == wyDto.Year);
        }

        public static WeekMenuDto MapWeekMenuDto(this IRepositoryAsync<MenuForWeek> repository, WeekYearDto wyDto)
        {
            MenuForWeek wmenu = repository.GetWeekMenuByWeekYear(wyDto);
            if (wmenu == null && repository.GetAll().Count == 0)
            {
                repository.Context.CreateNewWeekMenu(wyDto);
                wmenu = repository.GetWeekMenuByWeekYear(wyDto);
            }
            ;
            if (wmenu == null)
            {
                return null;
            }
            WeekMenuDto dtoModel = new WeekMenuDto
            {
                Id = wmenu.ID,
                SummaryPrice = wmenu.SummaryPrice,
                MfdModels =
                    wmenu.MenuForDay
                        .Select(mfd => new MenuForDayDto
                        {
                            Id = mfd.ID,
                            TotalPrice = mfd.TotalPrice,
                            Dishes =
                                mfd.DishPriceMfdRelations.Select(dp => dp.Dish).OrderBy(d => d.DishType.Id).Select(d =>
                                {
                                    var mfdDishPriceRelations =
                                        mfd.DishPriceMfdRelations.FirstOrDefault(dp => dp.DishId == d.DishID);
                                    if (mfdDishPriceRelations != null)
                                        return new DTO.SuperUser.Dishes.DishModelDto
                                        {
                                            DishId = d.DishID,
                                            Title = d.Title,
                                            ProductImage = d.ProductImage,
                                            Price =
                                                mfdDishPriceRelations
                                                    .DishPrice.Price,
                                            Category = mfdDishPriceRelations.Dish.DishType.Category,
                                            Description = mfdDishPriceRelations.Dish.Description
                                        };
                                    return null;
                                }).ToList(),
                            OrderCanBeChanged = mfd.OrderCanBeChanged,
                            OrderWasBooking =
                                repository.GetRepositoryAsync<WeekOrderMenu>().GetUsersMadeOrder(mfd.ID).Any(),
                            DayMenuCanBeChanged = mfd.DayMenuCanBeChanged
                        })
                        .ToList(),
                OrderCanBeCreated = wmenu.OrderCanBeCreated,
                WorkWeekDays = wmenu.WorkingWeek.WorkingDays.Select(wd => wd.IsWorking).ToArray(),
                WorkingDaysAreSelected = wmenu.WorkingDaysAreSelected,
                DayNames = repository.Context.GetDayNames(wyDto).Result,
                WeekYear = wyDto
            };
            return dtoModel;
        }

        public static MenuForWeek GetWeekMenuMfdContains(this IRepositoryAsync<MenuForWeek> repository, int mfdid)
        {
            MenuForWeek mfw =
               repository.Query()
                   .Include(wm => wm.MenuForDay.Select(dm => dm.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                   .Include(wm => wm.MenuForDay.Select(dm => dm.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                   .Include(wm => wm.MenuForDay.Select(dm => dm.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                   .Include(wm => wm.Orders)
                   .Include(wm => wm.WorkingWeek.Year)
                   .Include(wm => wm.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                   .Select()
                   .FirstOrDefault(mw => mw.MenuForDay.Select(md => md.ID).Contains(mfdid));
            return mfw;
        }
    }
}