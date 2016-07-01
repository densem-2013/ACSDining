using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.DTO.Employee
{
    public class EmployeeWeekOrderDto
    {
        public int WeekOrderId { get; set; }
        //недельный заказ  клиента по дням
        public List<OrderDayMenuDto> DayOrders { get; set; }
        //недельный заказ по каждому блюду
        public double[] WeekOrderDishes { get; set; }
        public bool WeekIsPaid { get; set; }
        public double Balance { get; set; }
        public WeekYearDto WeekYear { get; set; }
        public double PrevWeekBalance { get; set; }
        public double WeekPaiment { get; set; }
        public double AllowDebt { get; set; }
        public bool CheckDebt { get; set; }
        public string[] DayNames { get; set; }
        public static EmployeeWeekOrderDto MapDto(ApplicationDbContext context, WeekPaiment weekPaiment,
            WeekYearDto wyDto)
        {
            double defaultDebt;
            double.TryParse(WebConfigurationManager.AppSettings["defaultCreditValue"], out defaultDebt);

            return new EmployeeWeekOrderDto
            {
                WeekOrderId = weekPaiment.WeekOrderMenu.Id,
                DayOrders =
                    weekPaiment.WeekOrderMenu.DayOrderMenus.Where(dom => dom.MenuForDay.WorkingDay.IsWorking)
                        .Select(OrderDayMenuDto.MapDto)
                        .ToList(),
                WeekOrderDishes = context.FactDishQuantByWeekOrderId(weekPaiment.WeekOrderMenu.Id).Result,
                WeekIsPaid = weekPaiment.WeekIsPaid,
                Balance = weekPaiment.WeekOrderMenu.User.Balance,
                WeekYear = wyDto,
                PrevWeekBalance = weekPaiment.PreviousWeekBalance,
                WeekPaiment = weekPaiment.Paiment,
                AllowDebt = defaultDebt,
                CheckDebt = weekPaiment.WeekOrderMenu.User.CheckDebt,
                DayNames = context.GetDayNames(wyDto).Result
            };
        }
    }
}
