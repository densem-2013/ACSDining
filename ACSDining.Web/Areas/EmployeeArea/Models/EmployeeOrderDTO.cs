using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ACSDining.Core.Domains;
using ACSDining.Web.Areas.SU_Area.Models;

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
        public List<MenuForDayModel> MFD_models { get; set; }
        public int Year { get; set; }
        public int WeekNumber { get; set; }


    }
}