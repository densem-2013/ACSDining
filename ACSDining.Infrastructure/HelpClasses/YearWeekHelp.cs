using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;
using DayOfWeek = System.DayOfWeek;

namespace ACSDining.Infrastructure.HelpClasses
{
    public static class YearWeekHelp
    {
        #region _acsContext Members

        public static Func<int> CurrentWeek = () =>
        {
            CultureInfo myCi = new CultureInfo("uk-UA");
            Calendar myCal = myCi.Calendar;

            // Gets the DTFI properties required by GetWeekOfYear.
            CalendarWeekRule myCwr = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek myFirstDow = myCi.DateTimeFormat.FirstDayOfWeek;
            DateTime curDay = DateTime.Now;
            return myCal.GetWeekOfYear(curDay, myCwr, myFirstDow);
        };

        public static DateTime DateOfWeekIso8601(int year, int weekOfYear, int daynum)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            var result = firstThursday.AddDays(weekNum * 7);

            return result.AddDays(-4 + daynum);
        }
        public static string GetWeekTitle(IRepositoryAsync<MenuForWeek> menuRepositoryAsync, WeekYearDto dto)
        {
            WorkingWeek workWeek = menuRepositoryAsync
                          .WorkWeekByWeekYear(dto);
            int mindayId = workWeek.WorkingDays.Where(wd => wd.IsWorking).Min(wd => wd.DayOfWeek.Id);
            int maxdayId = workWeek.WorkingDays.Where(wd => wd.IsWorking).Max(wd => wd.DayOfWeek.Id);
            DateTime firstday = DateOfWeekIso8601(dto.Year, dto.Week, mindayId);
            DateTime lastday = DateOfWeekIso8601(dto.Year, dto.Week, maxdayId);
            List<Core.Domains.DayOfWeek> days = menuRepositoryAsync.Context.Days.ToList();
            CultureInfo myCi = new CultureInfo("uk-UA");
            return string.Format("Неделя_{0}-{1}-{2}", dto.Week, firstday.Date.ToString("ddd_dd_MM_yy", myCi), lastday.Date.ToString("ddd_dd_MM_yy",myCi));
        }
        //Get Last Week Number of Year
        public static Func<int, int> YearWeekCount = (int year) =>
        {
            CultureInfo myCi = new CultureInfo("uk-UA");
            Calendar myCal = myCi.Calendar;

            // Gets the DTFI properties required by GetWeekOfYear.
            CalendarWeekRule myCwr = myCi.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDow = myCi.DateTimeFormat.FirstDayOfWeek;
            DateTime lastweek = new DateTime(year, 12, 31);
            return myCal.GetWeekOfYear(lastweek, myCwr, myFirstDow);
        };


        public static WeekYearDto GetNextWeekYear(WeekYearDto wyDto)
        {
            WeekYearDto result = new WeekYearDto();
            if (wyDto.Week >= 52)
            {
                DateTime lastDay = new DateTime(wyDto.Year, 12, 31);
                if (lastDay.DayOfWeek < DayOfWeek.Thursday || wyDto.Week == 53)
                {
                    result.Week = 1;
                    result.Year = wyDto.Year + 1;
                }
            }
            else
            {
                result.Week = wyDto.Week + 1;
                result.Year = wyDto.Year;
            }

            return result;
        }

        public static WeekYearDto GetPrevWeekYear(WeekYearDto wyDto)
        {
            WeekYearDto result = new WeekYearDto();
            if (wyDto.Week == 1)
            {
                result.Week = YearWeekCount(wyDto.Year);
                result.Year = wyDto.Year - 1;
            }
            else
            {
                result.Week = wyDto.Week - 1;
                result.Year = wyDto.Year;
            }

            return result;
        }



        public static bool WeekDaysCanBeChanged(WorkingWeek workweek)
        {
            WeekYearDto curDto = new WeekYearDto
            {
                Week = CurrentWeek(),
                Year = DateTime.Now.Year
            };
            WeekYearDto nextDto = GetNextWeekYear(curDto);
            return (workweek.WeekNumber == curDto.Week && workweek.Year.YearNumber == curDto.Year) ||
                   (workweek.WeekNumber == nextDto.Week && workweek.Year.YearNumber == nextDto.Year);
        }

        public static bool WeekIsCurrentOrNext(WeekYearDto wyDto)
        {
            WeekYearDto nextWeekYearDto = GetNextWeekYear(GetCurrentWeekYearDto());

            return (wyDto.Week == CurrentWeek() && wyDto.Year == DateTime.Now.Year) ||
                   (wyDto.Week == nextWeekYearDto.Week && wyDto.Year == nextWeekYearDto.Year);
        }

        public static WeekYearDto GetCurrentWeekYearDto()
        {
            return new WeekYearDto
            {
                Week = CurrentWeek(),
                Year = DateTime.Now.Year
            };
        }

        public static bool IsCurrentWeekYearDto(WeekYearDto wyDto)
        {
            WeekYearDto curwyDto = GetCurrentWeekYearDto();
            return curwyDto.Week == wyDto.Week && curwyDto.Year == wyDto.Year;

        }
        #endregion
    }
}
