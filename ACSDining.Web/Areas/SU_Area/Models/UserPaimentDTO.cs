using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class UserPaimentDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public double[] Paiments { get; set; }
        public double SummaryPrice { get; set; }
        public double WeekPaid { get; set; }
        public double Balance { get; set; }
    }
}