using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Infrastructure.DTO
{
    public class UpdateAllWeekOrderDto
    {
        public int[] DayOrdIds { get; set; }
        public int WeekOrdId { get; set; }
        public double[] QuantArray { get; set; }
    }
}
