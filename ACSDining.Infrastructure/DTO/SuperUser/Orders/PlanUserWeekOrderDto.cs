using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.DTO.SuperUser.Orders
{
    public class PlanUserWeekOrderDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int OrderId { get; set; }
        public int[] DayOrdIdArray { get; set; }
        public double[] UserWeekOrderDishes { get; set; }

        /// <param name="context"></param>
        /// <param name="planWeekOrderMenu"></param>
        /// <returns></returns>
        public static PlanUserWeekOrderDto MapDto(ApplicationDbContext context, PlannedWeekOrderMenu planWeekOrderMenu)
        {
            return new PlanUserWeekOrderDto
            {
                UserId = planWeekOrderMenu.User.Id,
                UserName = string.Format("{0} {1}", planWeekOrderMenu.User.LastName, planWeekOrderMenu.User.FirstName),
                OrderId = planWeekOrderMenu.Id,
                DayOrdIdArray =
                    planWeekOrderMenu.PlannedDayOrderMenus.Where(dord => dord.MenuForDay.WorkingDay.IsWorking)
                        .Select(dord => dord.Id)
                        .ToArray(),
                UserWeekOrderDishes = context.PlanDishQuantByWeekOrderId(planWeekOrderMenu.Id).Result
            };
        }
    }
}
