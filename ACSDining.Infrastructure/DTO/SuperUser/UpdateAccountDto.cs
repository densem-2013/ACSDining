using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class UpdateAccountDto
    {
        public string UserId { get; set; }
        public double Balance { get; set; }
        //Пользователь может делать заказ
        public bool CanMakeBooking { get; set; }
        //Пользователь существует( не удалён из Active Directory)
        public bool IsExisting { get; set; }
    }
}
