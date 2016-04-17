using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.Employee
{
    //Представляет информацию о фактической заявке пользователя на неделю
    public class UserWeekOrderDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int OrderId { get; set; }
        public List<UserDayOrderDto> DayOrderDtos { get; set; }
        public double WeekSummaryPrice { get; set; }
        public double WeekPaid { get; set; }
        public bool WeekIsPaid { get; set; }
        public WeekYearDto WeekYear { get; set; }

        /// <param name="unitOfWork"></param>
        /// <param name="weekOrderMenu"></param>
        /// <param name="catLength">Количество категорий блюд</param>
        /// <param name="forWeekOrders">Если преобразование происходит для OrderApiController</param>
        /// <returns></returns>
        public static UserWeekOrderDto MapDto(IUnitOfWorkAsync unitOfWork, WeekOrderMenu weekOrderMenu, int catLength,
            bool forWeekOrders = false)
        {
            return new UserWeekOrderDto
            {
                WeekPaid = weekOrderMenu.WeekPaid,
                UserId = weekOrderMenu.User.Id,
                UserName = weekOrderMenu.User.UserName,
                OrderId = weekOrderMenu.Id,
                DayOrderDtos =
                    weekOrderMenu.DayOrderMenus.Where(dord =>
                        dord.MenuForDay.WorkingDay.IsWorking).Select(
                            dord => UserDayOrderDto.MapUserDayOrderDto(unitOfWork, dord, catLength, forWeekOrders))
                        .ToList(),
                WeekSummaryPrice = weekOrderMenu.WeekOrderSummaryPrice,
                WeekYear = WeekYearDto.MapDto(weekOrderMenu.MenuForWeek.WorkingWeek)
            };
        }
    }
}
