using System;
using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;
using LinqKit;

namespace ACSDining.Infrastructure.Repositories
{
    public static class MenuForWeekRepository
    {

        public static double[] UnitWeekPricesByWeekYear(this IRepositoryAsync<MenuForWeek> repository, WeekYearDto wyDto,
            int catLenth)
        {

            MenuForWeek mfw = repository.GetWeekMenuByWeekYear(wyDto);

            WorkingWeek workingWeek = repository.WorkWeekByWeekYear(wyDto);
            int dayCount = workingWeek.WorkingDays.Count(d => d.IsWorking);
            int arLenth = dayCount*catLenth;

            double[] unitprices = new double[arLenth];

            for (int i = 0; i < dayCount; i++)
            {
                MenuForDay daymenu = mfw.MenuForDay.OrderBy(mfd => mfd.WorkingDay.DayOfWeek.Id).ElementAt(i);
                for (int j = 0; j < catLenth; j++)
                {
                    unitprices[i*catLenth + j] = daymenu.Dishes.OrderBy(d => d.DishType.Id).ElementAt(j).Price;
                }
            }
            return unitprices;
        }

        public static MenuForWeek GetWeekMenuByWeekYear(this IRepositoryAsync<MenuForWeek> repository, WeekYearDto wyDto)
        {
            MenuForWeek mfw =
                repository.Query()
                    .Include(wm => wm.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                    .Include(wm => wm.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishDetail)))
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
                    .Include(wm => wm.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                    .Include(wm => wm.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishDetail)))
                    .Include(wm => wm.Orders)
                    .Include(wm => wm.WorkingWeek.Year)
                    .Include(wm => wm.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                    .Select()
                    .FirstOrDefault(wm => wm.ID == menuid);

            return mfw;
        }

        public static List<int> GetWeekNumbers(this IRepositoryAsync<MenuForWeek> repository)
        {
            List<MenuForWeek> list = repository.Query().Include(mfw => mfw.WorkingWeek.Year).Select().ToList();
            List<int> years = list.Select(wm => wm.WorkingWeek.Year.YearNumber).Distinct().ToList();
            years.Sort();
            List<int> numweeks = new List<int>();

            foreach (int year in years)
            {
                var yearweeks =
                    list.Where(m => m.WorkingWeek.Year.YearNumber == year)
                        .Select(wm => wm.WorkingWeek.WeekNumber)
                        .ToList();
                yearweeks.Sort();
                numweeks = numweeks.Concat(yearweeks).ToList();
            }

            return
                repository.Query()
                    .Include(m => m.WorkingWeek)
                    .Select(wm => wm.WorkingWeek.WeekNumber)
                    .Reverse()
                    .ToList();
        }

        /// <summary>
        /// Запрашивает объект MenuForWeek из базы
        /// Если объект не существует, проверяет является ли запрашиваемый объект меню на следующую неделю или на текущую неделю в системе
        /// Если является то создаётся новое меню (пустое), сохраняет его в базе и отправляет его DTO клиенту
        /// Если запрашивается меню на новую рабочую неделю, которой ещё нет в базе, создаётся новая рабочая неделя с рабочими 
        /// днями Понедельник - Пятница
        /// Если год не существует в базе, создаётся новый
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="weekyear"></param>
        /// <returns></returns>
        public static MenuForWeek CreateMenuForWeekOnWeekYear(this IRepositoryAsync<MenuForWeek> repository,
            WeekYearDto weekyear)
        {
            List<WorkingDay> workdays = new List<WorkingDay>();

            IRepositoryAsync<Year> yearRepository = repository.GetRepositoryAsync<Year>();

            Year year = yearRepository.GetAll().FirstOrDefault(y => y.YearNumber == weekyear.Year) ??
                        new Year {YearNumber = weekyear.Year};

            List<Core.Domains.DayOfWeek> daysWeeks = repository.GetRepositoryAsync<Core.Domains.DayOfWeek>().GetAll();

            for (int i = 0; i < daysWeeks.Count; i++)
            {
                WorkingDay wday = new WorkingDay
                {
                    DayOfWeek = daysWeeks.ElementAt(i),
                    IsWorking = i < 5
                };
                workdays.Add(wday);
            }

            WorkingWeek workWeek = repository.WorkWeekByWeekYear(weekyear);

            if (workWeek == null)
            {
                workWeek = new WorkingWeek
                {
                    WeekNumber = weekyear.Week,
                    Year = year,
                    WorkingDays = workdays,
                    CanBeChanged = true
                };
            }


            if (year.WorkingWeeks.FirstOrDefault(ww => ww.WeekNumber == workWeek.WeekNumber) == null)
            {
                year.WorkingWeeks.Add(workWeek);
            }
            List<MenuForDay> mfdays = new List<MenuForDay>();
            for (var i = 0; i < 7; i++)
            {
                {
                    WorkingDay wday = workWeek.WorkingDays.FirstOrDefault(wd => wd.DayOfWeek.Id == i + 1);
                    if (wday != null /*&& wday.IsWorking*/)
                    {
                        MenuForDay mfd = new MenuForDay
                        {
                            WorkingDay = wday,
                            DayMenuCanBeChanged = true,
                            OrderCanBeChanged = true
                        };
                        mfdays.Add(mfd);
                    }
                }
            }

            MenuForWeek weekmenu = new MenuForWeek
            {
                WorkingWeek = workWeek,
                MenuForDay = mfdays
            };


            return weekmenu;
        }


        public static WorkingWeek WorkWeekByWeekYear(this IRepositoryAsync<MenuForWeek> repository, WeekYearDto wyDto)
        {
            return repository.GetRepositoryAsync<WorkingWeek>().Query()
                .Include(ww => ww.WorkingDays.Select(wd => wd.DayOfWeek))
                .Include(ww => ww.Year)
                .Select()
                .FirstOrDefault(ww => ww.WeekNumber == wyDto.Week && ww.Year.YearNumber == wyDto.Year);
        }

        public static void DayUpdates(this IRepositoryAsync<WorkingWeek> repository, WorkWeekDto weekModel)
        {

            WorkingWeek week = repository.Find(weekModel.WorkWeekId);

            week.WorkingDays.ForEach(x =>
            {
                var firstOrDefault = weekModel.WorkDays.FirstOrDefault(wd => wd.WorkdayId == x.Id);
                var isWorking = firstOrDefault != null && firstOrDefault.IsWorking;
                x.IsWorking = isWorking;
            });
            repository.Update(week);
        }
    }
}