using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;

namespace ACSDining.Infrastructure.DTO.Employee
{
    public class OrderDayMenuDto
    {
        public int DayOrdId { get; set; }
        public List<OrderDishDto> Dishes { get; set; }
        public bool OrderCanByChanged { get; set; }
        public double DayOrderSummary { get; set; }
        public string DayName { get; set; }

        public static OrderDayMenuDto MapDto(DayOrderMenu dayorder)
        {
            return new OrderDayMenuDto
            {
                DayOrdId = dayorder.Id,
                Dishes =
                    dayorder.MenuForDay.DishPriceMfdRelations.OrderBy(d => d.Dish.DishType.Id)
                        .Select(dp => new OrderDishDto
                        {
                            Title = dp.Dish.Title,
                            Description = dp.Dish.Description,
                            Price = dp.DishPrice.Price,
                            Category = dp.Dish.DishType.Category

                        }).ToList(),
                OrderCanByChanged = dayorder.MenuForDay.OrderCanBeChanged,
                DayOrderSummary = dayorder.DayOrderSummaryPrice,
                DayName = dayorder.MenuForDay.WorkingDay.DayOfWeek.Name
            };
        }
    }
}
