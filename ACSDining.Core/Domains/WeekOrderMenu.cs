//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ACSDining.Core.Domains
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WeekOrderMenu 
    {
        public WeekOrderMenu()
        {
            DayOrderMenus = new List<DayOrderMenu>();
        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public double WeekPaid { get; set; }
        public double Balance { get; set; }
        public double WeekOrderSummaryPrice { get; set; }
        public string Note { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [JsonIgnore]
        public virtual MenuForWeek MenuForWeek { get; set; }
        [JsonIgnore]
        public virtual ICollection<DayOrderMenu> DayOrderMenus { get; set; }

    }
}
