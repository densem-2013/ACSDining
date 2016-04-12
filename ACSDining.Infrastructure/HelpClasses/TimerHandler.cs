using System;
using System.Linq;
using System.Threading;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.HelpClasses
{
    public static class TimerHandler
    {
        public static void Init()
        {
            StartChecking();
            // In some constructor or method that runs when the app starts.
            // 1st parameter is the callback to be run every iteration.
            // 2nd parameter is optional parameters for the callback.
            // 3rd parameter is telling the timer when to start.
            // 4th parameter is telling the timer how often to run.
            Timer timer = new Timer(TimerElapsed, null, new TimeSpan(0), new TimeSpan(1, 0, 0));
        }

        // The callback, no inside the method used above.
        // This will run every  hour.
        private static void TimerElapsed(object o)
        {
            if (DateTime.Now.Hour == 9)
            {
                WeekYearDto wyDto = YearWeekHelp.GetCurrentWeekYearDto();
                using (ApplicationDbContext _db = new ApplicationDbContext())
                {
                    MenuForDay dayMenu =
                        _db.MenuForDays.FirstOrDefault(
                            mfd =>
                                mfd.WorkingDay.WorkingWeek.Year.YearNumber == wyDto.Year &&
                                mfd.WorkingDay.WorkingWeek.WeekNumber == wyDto.Week &&
                                mfd.WorkingDay.DayOfWeek.Id == (int) DateTime.Now.DayOfWeek);

                    if (dayMenu != null)
                    {
                        dayMenu.OrderCanBeChanged = false;
                        dayMenu.DayMenuCanBeChanged = false;
                    }
                }
            }
        }

        /// <summary>
        /// Отмена редактирования меню и заказов у которых истёк срок для редактирования
        /// Для меню на день - это 9.00 текущего дня в системе
        /// </summary>
        private static void StartChecking()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                int year = DateTime.Now.Year;
                int day = (int) DateTime.Now.DayOfWeek;
                int week = YearWeekHelp.CurrentWeek();
                context.MenuForWeeks.Include("MenuForDay.WorkingWeek").Include("WorkingWeek.Year").ToList().ForEach(x =>
                {
                    if (x.WorkingWeek.Year.YearNumber == year &&
                        (x.WorkingWeek.WeekNumber == week || x.WorkingWeek.WeekNumber == week + 1))
                    {
                        x.OrderCanBeCreated = true;
                        x.MenuCanBeChanged = true;
                        x.MenuForDay.ToList().ForEach(y =>
                        {
                            if (y.ID>day)
                            {
                                y.OrderCanBeChanged = true;
                                y.DayMenuCanBeChanged = true;
                            }
                            if (y.ID==day)
                            {
                                y.OrderCanBeChanged = DateTime.Now.Hour < 9;
                                y.DayMenuCanBeChanged = DateTime.Now.Hour < 9;
                            }
                            if (y.ID<day)
                            {
                                y.OrderCanBeChanged = false;
                                y.DayMenuCanBeChanged = false;
                            }
                        });
                    }
                    else
                    {
                        x.OrderCanBeCreated = false;
                        x.MenuCanBeChanged = false;
                        x.MenuForDay.ToList().ForEach(y =>
                        {
                            y.OrderCanBeChanged = false;
                            y.DayMenuCanBeChanged = false;
                        });
                    }
                });
            }

        }
    }
}
