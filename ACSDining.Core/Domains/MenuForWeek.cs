//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.ComponentModel;

namespace ACSDining.Core.Domains
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MenuForWeek 
    {
        public MenuForWeek()
        {
            MenuForDay = new List<MenuForDay>();
            Orders = new List<WeekOrderMenu>();
            PlannedOrderMenus = new List<PlannedWeekOrderMenu>();
            OrderCanBeCreated = false;
            MenuCanBeChanged = true;
        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public double SummaryPrice { get; set; }
        //�� ���� ����� ������� �����
        public bool OrderCanBeCreated { get; set; }
        //���� ����� ���� ��������
        public bool MenuCanBeChanged { get; set; }
        [Required]
        public virtual WorkingWeek WorkingWeek { get; set; }

        [JsonIgnore]
        public virtual ICollection<MenuForDay> MenuForDay { get; set; }

        [JsonIgnore]
        public virtual ICollection<WeekOrderMenu> Orders { get; set; }

        [JsonIgnore]
        public virtual ICollection<PlannedWeekOrderMenu> PlannedOrderMenus { get; set; }
    }
}
