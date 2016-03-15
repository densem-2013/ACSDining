using System.Collections.Generic;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Infrastructure.DTO.Employee
{
    public class EmployeeOrderDto
    {
        public string UserId { get; set; }
        public int MenuId { get;set; }
        public int? OrderId { get; set; }
        public double SummaryPrice { get; set; }
        public double WeekPaid { get; set; }
        public double[] Dishquantities { get; set; }
        public List<MenuForDayDto> MfdModels { get; set; }
        public int Year { get; set; }
        public int WeekNumber { get; set; }


    }
}