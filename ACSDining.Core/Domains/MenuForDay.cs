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
    using System.Data.Entity;
    using System.Threading.Tasks;
    
    public partial class MenuForDay
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public double TotalPrice { get; set; }

        [JsonIgnore]
        public virtual ICollection<Dish> Dishes { get; set; }
        [JsonIgnore]
        public virtual DayOfWeek DayOfWeek { get; set; }
    }
}
