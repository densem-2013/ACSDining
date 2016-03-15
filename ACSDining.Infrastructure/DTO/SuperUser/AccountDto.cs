using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using Newtonsoft.Json;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class AccountDto
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string LastLoginTime { get; set; }
        public string RegistrationDate { get; set; }
        public static AccountDto MapDto(User user)
        {
            return new AccountDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                LastLoginTime = user.LastLoginTime.ToShortDateString(),
                RegistrationDate = user.RegistrationDate.ToShortDateString()
            };
        }
    }
}
