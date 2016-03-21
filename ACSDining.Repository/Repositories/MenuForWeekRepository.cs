using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Repository.Repositories
{
    public static class MenuForWeekRepository
    {

        public static double[] GetUnitWeekPrices(this IRepositoryAsync<MenuForWeek> repository, int menuforweekid)
        {

            double[] unitprices = new double[20];

            string[] categories =
                repository.GetRepository<DishType>().Queryable().OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            MenuForWeek mfw = repository.Find(menuforweekid);
            for (int i = 0; i < 5; i++)
            {
                MenuForDay daymenu = mfw.MenuForDay.ElementAt(i);
                for (int j = 0; j < categories.Length; j++)
                {
                    unitprices[i*4 + j] = daymenu.Dishes.ElementAt(j).Price;
                }
            }
            return unitprices;
        }

        public static WeekMenuDto GetMapWeekMenuDto(this IRepositoryAsync<MenuForWeek> repository, MenuForWeek wmenu,
            bool emptyDishes = false)
        {
            if (wmenu == null) return null;
            WeekMenuDto dtoModel = new WeekMenuDto
            {
                ID = wmenu.ID,
                WeekNumber = wmenu.WorkingWeek.WeekNumber,
                SummaryPrice = wmenu.SummaryPrice,
                YearNumber = wmenu.WorkingWeek.Year.YearNumber
            };
            if (emptyDishes)
            {
                IQueryable<DishType> dtypes = repository.GetRepository<DishType>().Queryable();
                dtoModel.MFD_models = new List<MenuForDayDto>();
                foreach (MenuForDay mfd in wmenu.MenuForDay)
                {
                    var dmodels = new List<DishModelDto>();
                    for (int i = 0; i < 4; i++)
                    {
                        DishType firstOrDefault = dtypes.FirstOrDefault(dt => dt.Id == i + 1);
                        if (firstOrDefault != null)
                            dmodels.Add(new DishModelDto
                            {
                                DishID = i + 1,
                                Title = "_",
                                Price = 0,
                                Category = firstOrDefault.Category,
                                Foods = "_"
                            });
                    }

                    dtoModel.MFD_models.Add(new MenuForDayDto
                    {
                        ID = mfd.ID,
                        DayOfWeek = mfd.WorkingDay.DayOfWeek.Name,
                        TotalPrice = mfd.TotalPrice,
                        Dishes = dmodels
                    });
                }
            }
            else
            {
                try
                {

                    dtoModel.MFD_models = wmenu.MenuForDay.ToList().Select(MenuForDayDto.MapDto).ToList();
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            return dtoModel;
        }

        public static WeekMenuDto GetWeekMenuDtoByWeekYear(this IRepositoryAsync<MenuForWeek> repository, int numweek,
            int year)
        {
            MenuForWeek mfw =
                repository.Queryable()
                    .FirstOrDefault(wm => wm.WorkingWeek.WeekNumber == numweek && wm.WorkingWeek.Year.YearNumber == year);

            return repository.GetMapWeekMenuDto(mfw);
        }

        public static List<int> GetWeekNumbers(this IRepositoryAsync<MenuForWeek> repository)
        {
            return repository.Queryable().Select(wm => wm.WorkingWeek.WeekNumber).Reverse().ToList();
        }
    }
}
