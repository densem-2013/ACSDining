using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.UnitOfWork;

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
        public static UserDayOrderDto MapUserDayOrderDto(/*IUnitOfWorkAsync unitOfWork, */DayOrderMenu dayOrderMenu/*, bool forWeekOrders = false*/)
        {
            //WorkingDay workday = dayOrderMenu.MenuForDay.WorkingDay;

            //List<DishQuantityRelations> quaList = unitOfWork.Repository<DishQuantityRelations>()
            //    .Query()
            //    .Include(dq => dq.DishQuantity)
            //    .Include(dq => dq.MenuForDay.WorkingDay.DayOfWeek)
            //    .Include(dq => dq.DayOrderMenu.MenuForDay.WorkingDay.DayOfWeek)
            //    .Select()
            //    .Where(
            //        dqr =>
            //            dqr.DayOrderMenuId == dayOrderMenu.Id&& dqr.MenuForDayId == dayOrderMenu.MenuForDay.ID )
            //            .OrderBy(dq=>dq.DishTypeId)
            //            .ToList();

            //double[] dquantities = new double[catlength];


            //for (int j = 1; j <= catlength; j++)
            //{
            //    var firstOrDefault = quaList.FirstOrDefault(
            //        q => q.MenuForDay.WorkingDay.Id == workday.Id && q.DishTypeId == j);
            //    if (firstOrDefault != null)
            //        dquantities[j - 1] = firstOrDefault.DishQuantity.Quantity;
            //}
            return new UserDayOrderDto
            {
                DayOrderId = dayOrderMenu.Id,
                //MenuForDay = MenuForDayDto.MapDto(unitOfWork, dayOrderMenu.MenuForDay, forWeekOrders),
                //DishQuantities = dquantities,
                DayOrderSummary = dayOrderMenu.DayOrderSummaryPrice,
                OrderCanBeChanged = dayOrderMenu.MenuForDay.OrderCanBeChanged
            };
        }

    }
}
