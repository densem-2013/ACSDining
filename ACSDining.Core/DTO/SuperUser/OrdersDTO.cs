using System.Collections.Generic;

namespace ACSDining.Core.DTO.SuperUser
{
    public class OrdersDTO
    {
        public int WeekNumber { get; set; }
        public List<UserOrdesDTO> UserOrders { get; set; }
        public int YearNumber { get; set; }
    }
}