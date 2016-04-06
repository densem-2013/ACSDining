namespace ACSDining.Core.DTO.SuperUser
{
    public class UserPaimentDTO
    {
        public string UserId { get; set; }
        public int OrderId { get; set; }
        public string UserName { get; set; }
        // цены за каждое заказанное блюдо на неделе, заказанное клиентом, умноженное на заказанное количество
        public double[] Paiments { get; set; }
        //сумма недельного заказа
        public double SummaryPrice { get; set; }
        //неделя оплачена полностью
        public double WeekPaid { get; set; }
        //остаток по оплате
        public double Balance { get; set; }
        //примечание
        public string Note { get; set; }
    }
}