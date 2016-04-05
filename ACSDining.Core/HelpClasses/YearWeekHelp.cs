using System;
using System.Globalization;
using ACSDining.Core.Domains;
using ACSDining.Core.DTO.SuperUser;
using DayOfWeek = System.DayOfWeek;

namespace ACSDining.Core.HelpClasses
{
    public static class YearWeekHelp
    {
        #region _acsContext Members

        public static Func<int> CurrentWeek = () =>
        {
            CultureInfo myCi = new CultureInfo("uk-UA");
            Calendar myCal = myCi.Calendar;

            // Gets the DTFI properties required by GetWeekOfYear.
            CalendarWeekRule myCwr = myCi.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDow = myCi.DateTimeFormat.FirstDayOfWeek;
            DateTime curDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
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


        public static WeekYearDTO GetNextWeekYear(WeekYearDTO wyDto)
        {
            WeekYearDTO result = new WeekYearDTO();
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
        public static WeekYearDTO GetPrevWeekYear(WeekYearDTO wyDto)
        {
            WeekYearDTO result = new WeekYearDTO();
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
            WeekYearDTO curDto = new WeekYearDTO
            {
                Week = CurrentWeek(),
                Year = DateTime.Now.Year
            };
            WeekYearDTO nextDto = GetNextWeekYear(curDto);
            return (workweek.WeekNumber == curDto.Week && workweek.Year.YearNumber == curDto.Year) ||
                   (workweek.WeekNumber == nextDto.Week && workweek.Year.YearNumber == nextDto.Year);
        }
        #endregion
    }
}
