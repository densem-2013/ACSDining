namespace ACSDining.Infrastructure.DTO.SuperUser.Orders
{
    public class UpdateUserOrderDto
    {
        public int DayOrderId { get; set; }
        public int CategoryId { get; set; }
        public double Quantity { get; set; }
    }
}
