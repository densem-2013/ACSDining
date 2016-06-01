using ACSDining.Core.Domains;

namespace ACSDining.Infrastructure.DTO.SuperUser.Accounts
{
    public class AccountDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string LastLoginTime { get; set; }
        public double Balance { get; set; }
        public string RegistrationDate { get; set; }
        //Пользователь может делать заказ
        public bool CanMakeBooking { get; set; }
        //Пользователь существует( не удалён из Active Directory)
        public bool IsExisting { get; set; }

        public static AccountDto MapDto(User user)
        {
            return new AccountDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.LastName + " " + user.FirstName,
                LastLoginTime = user.LastLoginTime.ToShortDateString(),
                RegistrationDate = user.RegistrationDate.ToShortDateString(),
                CanMakeBooking = user.CanMakeBooking,
                IsExisting = user.IsExisting,
                Balance = user.Balance
            };
        }
    }
}
