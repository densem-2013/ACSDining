using System.Collections.Generic;

namespace ACSDining.Core.DTO.SuperUser
{
    public class OrdersDto
    {
        public int WeekNumber { get; set; }
        public List<UserOrdersDto> UserOrders { get; set; }
        public int YearNumber { get; set; }

    }
}
