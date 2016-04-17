using System;
using System.Globalization;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
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
            WeekYearDto nextWeekYearDto = GetNextWeekYear(new WeekYearDto
            {
                Week = CurrentWeek(),
                Year = DateTime.Now.Year
            });
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

        #endregion
    }
}
