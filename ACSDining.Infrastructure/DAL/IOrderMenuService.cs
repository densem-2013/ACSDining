using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Service
{
    public interface IOrderMenuService: IService<OrderMenu>
    {
        OrdersDTO OrdersDtoByWeekYear(int week, int year);
        OrderMenu GetOrderMenuByWeekYear(int week, int year);
        double SummaryPriceUserByWeekYear(UserOrdersDTO usorder, int numweek, int year);
        double[] UserWeekOrderDishes(int orderid);
        double[] UserWeekOrderPaiments(int orderid);
        EmployeeOrderDto EmployeeOrderByWeekYear(WeekMenuDto weekmodel, string userid, int numweek, int year);
        IQueryable<OrderMenu> GetAllByWeekYear(int numweek, int year);
        void UpdateOrderMenu(OrderMenu order);
        OrderMenu Find(int orderid);
    }
}