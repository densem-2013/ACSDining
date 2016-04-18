using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class WeekMenuDto
    {
        public int Id { get; set; }
        public WeekYearDto WeekYear { get; set; }
        public double SummaryPrice { get; set; }
        public List<MenuForDayDto> MfdModels { get; set; }
        //Это меню может быть изменено
        public bool MenuCanBeChanged { get; set; }
        //На это недельное меню может быть сделан заказ
        public bool OrderCanBeCreated { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="wmenu"></param>
        /// <param name="forWeekOrders">Если преобразование происходит для OrderApiController</param>
        /// <returns></returns>
        public static WeekMenuDto MapDto(IUnitOfWorkAsync unitOfWork, MenuForWeek wmenu,
            bool forWeekOrders = false)
        {
            if (wmenu == null) return null;
            WeekMenuDto dtoModel = new WeekMenuDto
            {
                Id = wmenu.ID,
                WeekYear = !forWeekOrders ? WeekYearDto.MapDto(wmenu.WorkingWeek) : null,
                SummaryPrice = wmenu.SummaryPrice,
                MfdModels =
                    wmenu.MenuForDay.ToList()
                        .Select(mfd => MenuForDayDto.MapDto(unitOfWork, mfd, forWeekOrders))
                        .ToList(),
                        MenuCanBeChanged = wmenu.MenuCanBeChanged,
                        OrderCanBeCreated = wmenu.OrderCanBeCreated
            };

            return dtoModel;
        }
    }

}