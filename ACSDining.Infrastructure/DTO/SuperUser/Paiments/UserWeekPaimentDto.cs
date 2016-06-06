using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.DTO.SuperUser.Paiments
{
    public class UserWeekPaimentDto
    {
        public int PaiId { get; set; }
        public string UserName { get; set; }
        //Неделя оплачена
        public bool WeekIsPaid { get; set; }
        //Фактическая оплата за недельный заказ
        public double Paiment { get; set; }
        //оплаты по каждому блюду в недельном заказе плюс итоговая сумма к оплате по заказу
        public double[] WeekPaiments { get; set; }
        //Баланс пользователя по оплате
        public double Balance { get; set; }
        //примечание
        public string Note { get; set; }
        //Баланс сза прошлую неделю
        public double PrevWeekBalance { get; set; }

        /// <param name="_db"></param>
        /// <param name="weekPaiment"></param>
        /// <returns></returns>
        public static UserWeekPaimentDto MapDto(ApplicationDbContext _db, WeekPaiment weekPaiment)
        {

            return new UserWeekPaimentDto
            {
                PaiId = weekPaiment.Id,
                Paiment = weekPaiment.Paiment,
                WeekIsPaid = weekPaiment.WeekIsPaid,
                UserName = weekPaiment.WeekOrderMenu.User.LastName + " " + weekPaiment.WeekOrderMenu.User.FirstName,
                WeekPaiments = _db.WeekPaimentByOrderId(weekPaiment.WeekOrderMenu.Id).Result,
                Balance = weekPaiment.WeekOrderMenu.User.Balance,
                Note = weekPaiment.Note,
                PrevWeekBalance=weekPaiment.PreviousWeekBalance
            };
        }
    }
}
