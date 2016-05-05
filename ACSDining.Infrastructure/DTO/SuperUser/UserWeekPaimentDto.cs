using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class UserWeekPaimentDto
    {
        public WeekYearDto WeekYear { get; set; }
        public string UserId { get; set; }
        public int OrderId { get; set; }
        public string UserName { get; set; }
        // цены за каждое заказанное блюдо на неделе, заказанное клиентом, умноженное на заказанное количество
        public List<UserDayPaiment> UserDayPaiments { get; set; }
        //сумма недельного заказа
        public double WeekSummaryPaiment { get; set; }
        //неделя оплачена полностью
        public double WeekPaid { get; set; }
        //остаток по оплате
        public double Balance { get; set; }
        //примечание
        public string Note { get; set; }

        /// <param name="unitOfWork"></param>
        /// <param name="weekOrderMenu"></param>
        /// <param name="catLength">Количество категорий блюд</param>
        /// <returns></returns>
        public static UserWeekPaimentDto MapDto(IUnitOfWorkAsync unitOfWork, WeekOrderMenu weekOrderMenu, int catLength)
        {
            List<UserDayPaiment> daypaiments = weekOrderMenu.DayOrderMenus.Select(
                dord => UserDayPaiment.MapDto(unitOfWork, dord, catLength)).ToList();

            return new UserWeekPaimentDto
            {
                WeekPaid = weekOrderMenu.WeekPaid,
                UserId = weekOrderMenu.User.Id,
                OrderId = weekOrderMenu.Id,
                UserName = weekOrderMenu.User.UserName,
                UserDayPaiments = daypaiments,
                WeekSummaryPaiment = daypaiments.Select(dp => dp.SummaryDayPaiment).Sum(),
                WeekYear = WeekYearDto.MapDto(weekOrderMenu.MenuForWeek.WorkingWeek),
                Balance = weekOrderMenu.User.Balance,
                Note = weekOrderMenu.Note
            };
        }
    }
}
