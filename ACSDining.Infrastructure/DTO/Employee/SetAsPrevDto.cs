using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Infrastructure.DTO.Employee
{
    public class SetAsPrevDto
    {
        public int PrevWeekOrdId { get; set; }
        public string[] DayNames { get; set; }
        public double[] Prevquants { get; set; }
    }
}
