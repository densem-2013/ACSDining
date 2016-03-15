using System.Collections.Generic;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class PaimentsDTO
    {
        public int WeekNumber { get; set; }
        public List<UserPaimentDTO> UserPaiments { get; set; }
        public int YearNumber { get; set; }
        public double[] UnitPrices { get; set; }
        public double[] UnitPricesTotal { get; set; }
    }
}