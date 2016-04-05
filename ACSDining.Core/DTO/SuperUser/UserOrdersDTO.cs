namespace ACSDining.Core.DTO.SuperUser
{
    public class UserOrdersDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public double[] Dishquantities { get; set; }
        public double SummaryPrice { get; set; }
        public double WeekPaid { get; set; }

    }
}