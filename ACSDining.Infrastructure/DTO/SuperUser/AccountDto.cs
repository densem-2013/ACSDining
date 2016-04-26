using ACSDining.Core.Domains;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class AccountDto
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string LastLoginTime { get; set; }
        public string RegistrationDate { get; set; }
        //Пользователь может делать заказ
        public string CanMakeBooking { get; set; }

        public static AccountDto MapDto(User user)
        {
            return new AccountDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                LastLoginTime = user.LastLoginTime.ToShortDateString(),
                RegistrationDate = user.RegistrationDate.ToShortDateString()
            };
        }
    }
}
