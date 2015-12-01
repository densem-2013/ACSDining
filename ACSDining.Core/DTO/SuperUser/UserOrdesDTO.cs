namespace ACSDining.Core.DTO.SuperUser
{
    public class UserOrdesDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public double[] Dishquantities { get; set; }
        public double SummaryPrice { get; set; }
        public bool WeekIsPaid { get; set; }

    }
}