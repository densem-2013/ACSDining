﻿using System.Collections.Generic;
using ACSDining.Core.DTO.SuperUser;

namespace ACSDining.Core.DTO.Employee
{
    public class EmployeeOrderDTO
    {
        public string UserId { get; set; }
        public int MenuId { get;set; }
        public int? OrderId { get; set; }
        public double SummaryPrice { get; set; }
        public double WeekPaid { get; set; }
        public double[] Dishquantities { get; set; }
        public List<MenuForDayDto> MFD_models { get; set; }
        public int Year { get; set; }
        public int WeekNumber { get; set; }


    }
}