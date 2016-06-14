using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Timers;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Identity;
using Timer = System.Threading.Timer;

namespace ACSDining.Infrastructure.HelpClasses
{
    public static class TimerHandler
    {
        public static void Init()
        {
            //StartChecking();
            //FakeStartChecking();
            // In some constructor or method that runs when the app starts.
            // 1st parameter is the callback to be run every iteration.
            // 2nd parameter is optional parameters for the callback.
            // 3rd parameter is telling the timer when to start.
            // 4th parameter is telling the timer how often to run.
            //Timer timer = new Timer(TimerElapsed, null, new TimeSpan(0), new TimeSpan(0, 1, 0));
            var timer = new System.Timers.Timer
            {
                Interval = 3600000, 
                Enabled = true
            };
            timer.Elapsed += TimerElapsed;
        }

        // The callback, no inside the method used above.
        // This will run every  hour.
        private static void TimerElapsed(object o, System.Timers.ElapsedEventArgs e)
        {
            if (e.SignalTime.Hour == 9)
            {
                using (ApplicationDbContext _db = new ApplicationDbContext())
                {
                        _db.DayFactToPlan();
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
                List<MenuForWeek> weekmenus =
                    context.MenuForWeeks.Include("MenuForDay").Include("WorkingWeek.Year").ToList();
                weekmenus.ForEach(x =>
                {
                    if (x.WorkingWeek.Year.YearNumber == year &&
                        (x.WorkingWeek.WeekNumber == week || x.WorkingWeek.WeekNumber == week + 1))
                    {
                        x.OrderCanBeCreated = true;
                       // x.MenuCanBeChanged = true;
                        context.Entry(x).State = EntityState.Modified;
                        x.MenuForDay.ToList().ForEach(y =>
                        {
                            if (y.ID > day)
                            {
                                y.DayMenuCanBeChanged = true;
                            }
                            if (y.ID == day)
                            {
                                y.DayMenuCanBeChanged = DateTime.Now.Hour < 9;
                            }
                            if (y.ID < day)
                            {
                                y.DayMenuCanBeChanged = false;
                            }
                            context.Entry(y).State = EntityState.Modified;
                        });
                    }
                    else
                    {
                        x.OrderCanBeCreated = false;
                       // x.MenuCanBeChanged = false;
                        context.Entry(x).State = EntityState.Modified;
                        x.MenuForDay.ToList().ForEach(y =>
                        {
                            y.DayMenuCanBeChanged = false;
                            context.Entry(y).State = EntityState.Modified;
                        });
                    }
                });
                context.SaveChanges();
            }

        }

        private static void FakeStartChecking()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                List<MenuForWeek> weekmenus =
                    context.MenuForWeeks.Include("MenuForDay").Include("WorkingWeek.Year").ToList();
                weekmenus.ForEach(x =>
                {
                    x.OrderCanBeCreated = true;
                   // x.MenuCanBeChanged = true;
                    x.MenuForDay.ToList().ForEach(y =>
                    {
                        y.DayMenuCanBeChanged = true;
                        context.Entry(y).State = EntityState.Modified;
                    });
                    context.Entry(x).State = EntityState.Modified;
                });
                context.SaveChanges();
            }
            
        }
    }
}
