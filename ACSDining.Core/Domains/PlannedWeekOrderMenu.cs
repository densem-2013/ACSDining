﻿using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public sealed class PlannedWeekOrderMenu
    {
        public PlannedWeekOrderMenu()
        {
            PlannedDayOrderMenus = new List<PlannedDayOrderMenu>();
        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonIgnore]
        public WeekOrderMenu WeekOrderMenu { get; set; }

        [JsonIgnore]
        public ICollection<PlannedDayOrderMenu> PlannedDayOrderMenus { get; set; }
    }
}
