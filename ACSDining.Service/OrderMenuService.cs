using System.Linq;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Repository.Repositories;

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

    public class OrderMenuService : Service<OrderMenu>, IOrderMenuService
    {
        private readonly IRepositoryAsync<OrderMenu> _repository;

        public OrderMenuService(IRepositoryAsync<OrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public OrdersDTO OrdersDtoByWeekYear(int week, int year)
        {
            return _repository.GetOrdersDtoByWeekYear(week, year);
        }

        public OrderMenu GetOrderMenuByWeekYear(int week, int year)
        {
            return _repository.OrderMenuByWeekYear(week, year);
        }

        public double SummaryPriceUserByWeekYear(UserOrdersDTO usorder, int numweek, int year)
        {
            return _repository.GetSummaryPriceUserByWeekYear(usorder, numweek, year);
        }

        public double[] UserWeekOrderDishes(int orderid)
        {
            return _repository.GetUserWeekOrderDishes(orderid);
        }

        public double[] UserWeekOrderPaiments(int orderid)
        {
            return _repository.GetUserWeekOrderPaiments(orderid);
        }
        
        public EmployeeOrderDto EmployeeOrderByWeekYear(WeekMenuDto weekmodel, string userid, int numweek, int year)
        {
            return _repository.EmployeeOrderByWeekYear(weekmodel, userid, numweek, year);
        }

        public IQueryable<OrderMenu> GetAllByWeekYear(int numweek, int year)
        {
            return _repository.Queryable().Where(
                        om => om.MenuForWeek.WorkingWeek.WeekNumber == numweek && om.MenuForWeek.WorkingWeek.Year.YearNumber == year);
        }

        public void UpdateOrderMenu(OrderMenu order)
        {
            _repository.Update(order);
        }

        public OrderMenu Find(int orderid)
        {
            return _repository.Find(orderid);
        }
    }
}
