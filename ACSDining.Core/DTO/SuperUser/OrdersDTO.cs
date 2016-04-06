using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;

namespace ACSDining.Core.DTO.SuperUser
{
    public class OrdersDTO
    {
        public int WeekNumber { get; set; }
        public List<UserOrdersDTO> UserOrders { get; set; }
        public int YearNumber { get; set; }

    }
}
