using System.Collections.Generic;

namespace ACSDining.Infrastructure.DTO.SuperUser.Menu
{
    public class WeekMenuDto
    {
        public int Id { get; set; }
        public WeekYearDto WeekYear { get; set; }
        public double SummaryPrice { get; set; }
        public List<MenuForDayDto> MfdModels { get; set; }
        //На это недельное меню может быть сделан заказ
        public bool OrderCanBeCreated { get; set; }
        //Представление информации о рабочей неделе
        public bool[] WorkWeekDays { get; set; }
        //Рабочие дни установлены
        public bool WorkingDaysAreSelected { get; set; }

        public string[] DayNames { get; set; }
   }

}