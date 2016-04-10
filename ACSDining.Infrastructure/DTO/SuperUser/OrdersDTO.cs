using System.Collections.Generic;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class OrdersDto
    {
        public int WeekNumber { get; set; }
        public List<UserWeekOrderDto> UserWeekOrderDtos { get; set; }
        //public List<UserOrdersDto> UserOrders { get; set; }
        public int YearNumber { get; set; }

    }
}
