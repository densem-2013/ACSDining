using ACSDining.Core.Domains;
using MenuForDayDto = ACSDining.Infrastructure.DTO.SuperUser.Menu.MenuForDayDto;

namespace ACSDining.Infrastructure.DTO.Employee
{
    //Заказ пользователя на день
    public class UserDayOrderDto
    {
        public int DayOrderId { get; set; }
        public MenuForDayDto MenuForDay { get; set; }
        public double[] DishQuantities { get; set; }
        public double DayOrderSummary { get; set; }
        public bool OrderCanBeChanged { get; set; }
        /// <summary>
        /// Получает представление фактической заявки
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="dayOrderMenu"></param>
        /// <param name="catlength"></param>
        /// <param name="forWeekOrders">Если преобразование происходит для OrderApiController</param>
        /// <returns></returns>
        public static UserDayOrderDto MapUserDayOrderDto(DayOrderMenu dayOrderMenu)
        {
            return new UserDayOrderDto
            {
                DayOrderId = dayOrderMenu.Id,
                DayOrderSummary = dayOrderMenu.DayOrderSummaryPrice,
                OrderCanBeChanged = dayOrderMenu.MenuForDay.OrderCanBeChanged
            };
        }

    }
}
