using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;
using NLog;

namespace ACSDining.Infrastructure.HelpClasses
{
    public static class TimerHandler
    {
        public static Logger Logger = LogManager.GetLogger("TimerHandler");

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

            try
            {
                Logger.Trace("Version: {0}", Environment.Version);
                Logger.Trace("OS: {0}", Environment.OSVersion);
                Logger.Trace("Command: {0}", Environment.CommandLine);

                NLog.Targets.FileTarget tar =
                    (NLog.Targets.FileTarget) LogManager.Configuration.FindTargetByName("run_log");
                tar.DeleteOldFileOnStartup = true;
            }
            catch (Exception e)
            {
                Logger.Error("Ошибка работы с логом: {0}", e.Message);
            }
        }

        // The callback, no inside the method used above.
        // This will run every  hour.
        private static void TimerElapsed(object o, System.Timers.ElapsedEventArgs e)
        {
            Logger.Trace("DateTime: {0}, e.SignalTime.Hour= {1}", DateTime.Now, e.SignalTime.Hour);
            if (e.SignalTime.Hour <= 9) return;
            int daynum = (int) e.SignalTime.DayOfWeek;
            IUnitOfWorkAsync unitofwork = DependencyResolver.Current.GetService<IUnitOfWorkAsync>();
            WorkingWeek currWorkingWeek =
                unitofwork.RepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(YearWeekHelp.GetCurrentWeekYearDto());

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                MenuForDay menuForDay =
                    db.MenuForDays.Include("WorkingDay.DayOfWeek").ToList()
                        .Where(mfd => currWorkingWeek.WorkingDays.Select(wd => wd.Id).Contains(mfd.WorkingDay.Id))
                        .FirstOrDefault(d => d.WorkingDay.DayOfWeek.Id == daynum);
                if (menuForDay != null  && (menuForDay.DayMenuCanBeChanged || menuForDay.OrderCanBeChanged))
                {
                    db.DayFactToPlan();
                    Logger.Trace("DayFactToPlan was executed !!!");
                }
            }
        }
    }
}
