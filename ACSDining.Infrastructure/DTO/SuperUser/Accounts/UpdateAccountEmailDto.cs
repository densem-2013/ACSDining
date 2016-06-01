namespace ACSDining.Infrastructure.DTO.SuperUser.Accounts
{
    public class UpdateAccountEmailDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        //Пользователь может делать заказ
        public bool CanMakeBooking { get; set; }
        //Пользователь существует( не удалён из Active Directory)
        public bool IsExisting { get; set; }
    }
}
