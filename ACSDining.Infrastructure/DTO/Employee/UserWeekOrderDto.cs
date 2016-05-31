using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.DTO.Employee
{
    //Представляет информацию о фактической заявке пользователя на неделю
    public class UserWeekOrderDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int OrderId { get; set; }
        public int[] DayOrdIdArray { get; set; }
        public double[] UserWeekOrderDishes { get; set; }

        /// <param name="context"></param>
        /// <param name="weekOrderMenu"></param>
        /// <returns></returns>
        public static UserWeekOrderDto MapDto(ApplicationDbContext context, WeekOrderMenu weekOrderMenu)
        {
            return new UserWeekOrderDto
            {
                UserId = weekOrderMenu.User.Id,
                UserName = string.Format("{0} {1}", weekOrderMenu.User.LastName, weekOrderMenu.User.FirstName),
                OrderId = weekOrderMenu.Id,
                DayOrdIdArray =
                    weekOrderMenu.DayOrderMenus.Where(dord => dord.MenuForDay.WorkingDay.IsWorking)
                        .Select(dord => dord.Id)
                        .ToArray(),
                UserWeekOrderDishes = context.FactDishQuantByWeekOrderId(weekOrderMenu.Id).Result 
            };
        }
    }
}
