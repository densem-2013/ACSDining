﻿using System;
using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Infrastructure.Repositories
{
    public static class MenuForWeekRepository
    {

        public static double[] GetUnitWeekPrices(this IRepositoryAsync<MenuForWeek> repository, int menuforweekid, string[] categories)
        {

            double[] unitprices = new double[20];

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
        
        public static MenuForWeek GetWeekMenuByWeekYear(this IRepositoryAsync<MenuForWeek> repository, WeekYearDto wyDto)
        {
            MenuForWeek mfw =
                repository.Query()
                    .Include(wm => wm.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                    .Include(wm => wm.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishDetail)))
                    .Include(wm => wm.Orders)
                    .Include(wm => wm.WorkingWeek.Year)
                    .Include(wm => wm.WorkingWeek.WorkingDays.Select(d=>d.DayOfWeek))
                    .Select()
                    .FirstOrDefault(wm => wm.WorkingWeek.WeekNumber == wyDto.Week && wm.WorkingWeek.Year.YearNumber == wyDto.Year);

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

            return repository.Query().Include(m=>m.WorkingWeek).Select(wm => wm.WorkingWeek.WeekNumber).Reverse().ToList();
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
        public static MenuForWeek CreateMenuForWeekOnWeekYear(this IRepositoryAsync<MenuForWeek> repository,WeekYearDto weekyear)
        {
            List<WorkingDay> workdays = new List<WorkingDay>();

            IRepositoryAsync<Year> yearRepository = repository.GetRepositoryAsync<Year>();

            Year year = yearRepository.GetAll().FirstOrDefault(y => y.YearNumber == weekyear.Year) ??
                        new Year { YearNumber = weekyear.Year };

            for (int i = 0; i < 7; i++)
            {
                WorkingDay wday = new WorkingDay
                {
                    DayOfWeek =repository.GetRepositoryAsync<Core.Domains.DayOfWeek>().FindAsync(i + 1).Result,
                    IsWorking = i < 5
                };
                workdays.Add(wday);
            }

            WorkingWeek workWeek = repository.GetRepositoryAsync<WorkingWeek>().WorkWeekByWeekYear(weekyear);

            if (workWeek == null)
            {
                workWeek = new WorkingWeek
                {
                    WeekNumber = weekyear.Week,
                    Year = year,
                    WorkingDays = workdays
                };
            }


            if (year.WorkingWeeks.FirstOrDefault(ww => ww.WeekNumber == workWeek.WeekNumber) == null)
            {
                year.WorkingWeeks.Add(workWeek);
            }
            //List<Dish> emptyDayDish=new List<Dish>();
            List<MenuForDay> mfdays=new List<MenuForDay>();
            for (var i = 0; i < 7; i++)
            {
                {
                    WorkingDay wday = workWeek.WorkingDays.FirstOrDefault(wd => wd.DayOfWeek.Id == i + 1);
                    if (wday != null && wday.IsWorking)
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

    }
}
