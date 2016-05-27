using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.DTO.Employee
{
    public class EmployeeWeekOrderDto
    {
        public int WeekOrderId { get; set; }
        //недельный заказ  клиента по дням
        public List<OrderDayMenuDto> DayOrders { get; set; }
        //названия рабочих дней недели
        public string[] DayNames { get; set; }
        //недельный заказ по каждому блюду
        public double[] WeekOrderDishes { get; set; }
        public bool WeekIsPaid { get; set; }
        public double Balance { get; set; }
        public WeekYearDto WeekYear { get; set; }
        public static EmployeeWeekOrderDto MapDto(ApplicationDbContext context, WeekPaiment weekPaiment,WeekYearDto wyDto)
        {
            return new EmployeeWeekOrderDto
            {
                WeekOrderId = weekPaiment.WeekOrderMenu.Id,
                DayOrders = weekPaiment.WeekOrderMenu.DayOrderMenus.Where(dom => dom.MenuForDay.WorkingDay.IsWorking).Select(OrderDayMenuDto.MapDto).ToList(),
                DayNames = context.GetDayNames(wyDto).Result,
                WeekOrderDishes = context.FactDishQuantByWeekOrderId(weekPaiment.WeekOrderMenu.Id).Result,
                WeekIsPaid = weekPaiment.WeekIsPaid,
                Balance = weekPaiment.WeekOrderMenu.User.Balance,
                WeekYear = wyDto
            };
        }
    }
}
