namespace ACSDining.Infrastructure.DTO.SuperUser.Accounts
{
    public class UpdateMakeBookDto
    {
        public string UserId { get; set; }
        //Пользователь может делать заказ
        public bool CanMakeBooking { get; set; }
    }
}
