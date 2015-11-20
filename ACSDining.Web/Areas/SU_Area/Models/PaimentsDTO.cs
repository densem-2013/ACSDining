using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class PaimentsDTO
    {
        public int WeekNumber { get; set; }
        public List<UserPaimentDTO> UserPaiments { get; set; }
        public int YearNumber { get; set; }
        public double[] UnitPrices { get; set; }
        public double[] UnitPricesTotal { get; set; }
    }
}