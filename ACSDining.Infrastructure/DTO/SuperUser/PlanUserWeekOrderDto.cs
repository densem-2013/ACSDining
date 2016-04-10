using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    //Представляет информацию о плановой заявке пользователя на неделю
    public class PlanUserWeekOrderDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<PlanUserDayOrderDto> PlanDayOrderDtos { get; set; }
        public double WeekSummaryPrice { get; set; }

        /// <param name="unitOfWork"></param>
        /// <param name="planWeekOrderMenu"></param>
        /// <param name="catLength">Количество категорий блюд</param>
        /// <returns></returns>
        public static PlanUserWeekOrderDto MapDto(IUnitOfWorkAsync unitOfWork, PlannedWeekOrderMenu planWeekOrderMenu, int catLength)
        {
            return new PlanUserWeekOrderDto
            {
                UserId = planWeekOrderMenu.WeekOrderMenu.User.Id,
                UserName = planWeekOrderMenu.WeekOrderMenu.User.UserName,
                PlanDayOrderDtos = planWeekOrderMenu.PlannedDayOrderMenus.Select(plandord => PlanUserDayOrderDto.MapUserDayOrderDto(unitOfWork, plandord, catLength)).ToList(),
                WeekSummaryPrice = planWeekOrderMenu.WeekOrderMenu.WeekOrderSummaryPrice
            };
        }
    }
}
