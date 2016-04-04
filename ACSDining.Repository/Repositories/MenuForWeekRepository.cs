using System;
using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DAL;
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
                repository.Query()
                    .Include(wm => wm.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                    .Include(wm => wm.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishDetail)))
                    .Include(wm => wm.Orders)
                    .Include(wm => wm.PlannedOrderMenus)
                    .Include(wm => wm.WorkingWeek.Year)
                    .Include(wm => wm.WorkingWeek.WorkingDays.Select(d=>d.DayOfWeek))
                    .Select()
                    .FirstOrDefault(wm => wm.WorkingWeek.WeekNumber == numweek && wm.WorkingWeek.Year.YearNumber == year);

            return repository.GetMapWeekMenuDto(mfw);
        }

        public static MenuForWeek WeekMenuByWeekYear(this IRepositoryAsync<MenuForWeek> repository, int numweek,
            int year)
        {
            MenuForWeek mfw =
                repository.Queryable()
                    .FirstOrDefault(wm => wm.WorkingWeek.WeekNumber == numweek && wm.WorkingWeek.Year.YearNumber == year);

            return mfw;
        }

        public static List<int> GetWeekNumbers(this IRepositoryAsync<MenuForWeek> repository)
        {
            List<WeekMenuDto> listDto = repository.Queryable().Select(x=>repository.GetMapWeekMenuDto(x,false)).ToList();
            List<int> years = listDto.Select(wm => wm.YearNumber).Distinct().ToList();
            years.Sort();
            List<int> numweeks = new List<int>();

            foreach (int year in years)
            {
                var yearweeks = listDto.Where(m => m.YearNumber == year).Select(wm => wm.WeekNumber).ToList();
                yearweeks.Sort();
                numweeks = numweeks.Concat(yearweeks).ToList();
            }

            return repository.Queryable().Select(wm => wm.WorkingWeek.WeekNumber).Reverse().ToList();
        }

        public static double GetSummaryPrice(this IRepositoryAsync<MenuForWeek> repository, UserOrdersDTO usorder, int numweek, int year)
        {
            MenuForWeek weekNeeded = repository.Queryable().FirstOrDefault(wm => wm.WorkingWeek.WeekNumber == numweek && wm.WorkingWeek.Year.YearNumber == year);
            double summary = 0;
            if (weekNeeded != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        summary += weekNeeded.MenuForDay.ElementAt(i).Dishes.ElementAt(j).Price *
                                   usorder.Dishquantities[4 * i + j];
                    }
                }
            }
            return summary;
        }

        public static WeekMenuDto GetNextWeekMenuByCurrentWekYear(this IRepositoryAsync<MenuForWeek> repository,WeekYearDTO weekyear)
        {
            WeekYearDTO nextweeknumber = UnitOfWork.GetNextWeekYear(weekyear);
            MenuForWeek nextWeek =
                repository.Queryable().FirstOrDefault(
                        mfw => mfw.WorkingWeek.WeekNumber == nextweeknumber.Week && mfw.WorkingWeek.Year.YearNumber == nextweeknumber.Year);
            WeekMenuDto dto;
            if (nextWeek != null)
            {
                dto = repository.GetMapWeekMenuDto(nextWeek);
                return dto;
            }
            WorkingWeek workingWeek = repository.GetRepository<WorkingWeek>().Queryable()
                .FirstOrDefault(w => w.WeekNumber == nextweeknumber.Week && w.Year.YearNumber == nextweeknumber.Year);
            var workdays = repository.GetRepository<WorkingDay>().Queryable();
            workdays.OrderBy(wd => wd.ID);
            nextWeek = new MenuForWeek
            {
                WorkingWeek = workingWeek,
                MenuForDay = workdays.Select(day => new MenuForDay
                {
                    WorkingWeek = workingWeek,
                    WorkingDay =workdays.FirstOrDefault(
                            wd => wd.WorkingWeek.ID == workingWeek.ID && wd.DayOfWeek.ID == day.ID)

                }).ToList()

            };

            return repository.GetMapWeekMenuDto(nextWeek, true);
        }

        public static void UpdateMenuForDay(this IRepositoryAsync<MenuForWeek> repository, MenuForDayDto menuforday)
        {
            List<Dish> dishes =
                menuforday.Dishes.SelectMany(d => repository.GetRepository<Dish>().Queryable().Where(dish => dish.DishID == d.DishID)).ToList();

            MenuForDay menuFd = repository.GetRepository<MenuForDay>().Find(menuforday.ID);
            menuFd.Dishes = dishes;
            menuFd.TotalPrice = menuforday.TotalPrice;
            repository.GetRepository<MenuForDay>().Update(menuFd);

            MenuForWeek mfwModel = repository.Queryable().FirstOrDefault(mfw => mfw.MenuForDay.Any(mfd => mfd.ID == menuforday.ID));

            mfwModel.SummaryPrice = mfwModel.MenuForDay.Sum(mfd => mfd.TotalPrice);

            repository.Update(mfwModel);
        }

        public static MenuForWeek CreateNextWeekMenu(this IRepositoryAsync<MenuForWeek> repository, WeekYearDTO weekyear)
        {
            WeekYearDTO nextweekDto = UnitOfWork.GetNextWeekYear(weekyear);
            Year nextYear =
                repository.GetRepository<Year>().Queryable().FirstOrDefault(y => y.YearNumber == nextweekDto.Year) ??
                new Year
                {
                    YearNumber = nextweekDto.Year
                };

            WorkingWeek nextworkingWeek =
                repository.GetRepository<WorkingWeek>().Queryable().FirstOrDefault(
                    w => w.WeekNumber == nextweekDto.Week && w.Year.YearNumber == nextweekDto.Year);
            if (nextworkingWeek==null)
            {
                nextworkingWeek = new WorkingWeek
                {
                    WeekNumber = nextworkingWeek.WeekNumber,
                    Year = nextYear
                };
            }
            MenuForWeek nextWeek = new MenuForWeek
            {
                WorkingWeek = nextworkingWeek,
                //Year =
                //    _yearRepository.Find(y => y.YearNumber == DateTime.Now.Year) ??
                //    new Year { YearNumber = nextweekDto.Year },
                MenuForDay = nextworkingWeek.WorkingDays.Select(day => new MenuForDay
                {
                    WorkingDay =
                        repository.GetRepository<WorkingDay>().Queryable().FirstOrDefault(
                            wd => wd.WorkingWeek.ID == nextworkingWeek.ID && wd.DayOfWeek.ID == day.ID),
                    WorkingWeek = nextworkingWeek
                }).ToList()

            };

            return nextWeek;
        }
    }
}
