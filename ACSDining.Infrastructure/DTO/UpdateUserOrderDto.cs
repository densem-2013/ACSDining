using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Infrastructure.DTO
{
    public class UpdateUserOrderDto
    {
        public int DayOrderId { get; set; }
        public int CategoryId { get; set; }
        public double Quantity { get; set; }
    }
}
