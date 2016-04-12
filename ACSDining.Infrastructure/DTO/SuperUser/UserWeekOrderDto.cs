using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    //Представляет информацию о фактической заявке пользователя на неделю
    public class UserWeekOrderDto
    {
        public string UserId { get; set; }
        public int OrderId { get; set; }
        public string UserName { get; set; }
        public List<UserDayOrderDto> DayOrderDtos { get; set; }
        public double WeekSummaryPrice { get; set; }
        public double WeekPaid { get; set; }
        public WeekYearDto WeekYear { get; set; }

        /// <param name="unitOfWork"></param>
        /// <param name="weekOrderMenu"></param>
        /// <param name="catLength">Количество категорий блюд</param>
        /// <returns></returns>
        public static UserWeekOrderDto MapDto(IUnitOfWorkAsync unitOfWork, WeekOrderMenu weekOrderMenu, int catLength)
        {
            return new UserWeekOrderDto
            {
                WeekPaid = weekOrderMenu.WeekPaid,
                UserId = weekOrderMenu.User.Id,
                OrderId = weekOrderMenu.Id,
                UserName = weekOrderMenu.User.UserName,
                DayOrderDtos =
                    weekOrderMenu.DayOrderMenus.Select(
                        dord => UserDayOrderDto.MapUserDayOrderDto(unitOfWork, dord, catLength)).ToList(),
                WeekSummaryPrice = weekOrderMenu.WeekOrderSummaryPrice,
                WeekYear = WeekYearDto.MapDto(weekOrderMenu.MenuForWeek.WorkingWeek)
            };
        }
    }
}
