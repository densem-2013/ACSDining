using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class UserOrdesDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public double[] Dishquantities { get; set; }
        public bool WeekIsPaid { get; set; }

    }
}