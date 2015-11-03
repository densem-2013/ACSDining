using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ACSDining.Core.Domains;

namespace ACSDining.Web.Areas.EmployeeArea.Models
{
    public class EmployeeOrderDTO
    {
        public string UserId { get; set; }
        public int MenuId { get;set; }
        public int? OrderId { get; set; }
        public double SummaryPrice { get; set; }
        public bool WeekIsPaid { get; set; }
        public double[] Dishquantities { get; set; }

       
    }
}