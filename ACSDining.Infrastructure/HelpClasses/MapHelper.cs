using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Infrastructure.HelpClasses
{
    public static class MapHelper
    {
        public static UserWeekOrderDto MapUserWeekOrderDto(IUnitOfWorkAsync unitOfWorkAsync, WeekOrderMenu dordMenu)
        {
            return new UserWeekOrderDto
            {
                WeekPaid = dordMenu.WeekPaid,
                UserId = dordMenu.User.Id,
                UserName = dordMenu.User.UserName,
                DayOrderDtos = dordMenu.DayOrderMenus.ToList().Select(dom =>
                {
                    return new UserDayOrderDto
                    {
                        DayOrderSummary = dom.DayOrderSummaryPrice,
                        DayId = dom.MenuForDay.WorkingDay.DayOfWeek.ID,
                        DayName = dom.MenuForDay.WorkingDay.DayOfWeek.Name
                    };
                }).ToList()
            };
        }
    }
}
