namespace ACSDining.Core.DTO.SuperUser
{
    public class UserPaimentDTO
    {
        public string UserId { get; set; }
        public int OrderId { get; set; }
        public string UserName { get; set; }
        public double[] Paiments { get; set; }
        public double SummaryPrice { get; set; }
        public double WeekPaid { get; set; }
        public double Balance { get; set; }
        public string Note { get; set; }
    }
}