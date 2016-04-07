using System.Collections.Generic;

namespace ACSDining.Core.DTO.SuperUser
{
    public class PaimentsDto
    {
        public int WeekNumber { get; set; }
        public List<UserPaimentDto> UserPaiments { get; set; }
        public int YearNumber { get; set; }
        //Цены за  каждое блюдо в меню на рабочей неделе
        public double[] UnitPrices { get; set; }
        //Суммы цен(т.е. стоимость каждого блюда, умноженное на заказанное количество на неделе всеми пользователями) по каждому блюду в меню
        public double[] UnitPricesTotal { get; set; }
    }
}