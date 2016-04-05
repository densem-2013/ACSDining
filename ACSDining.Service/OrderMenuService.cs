using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Repository.Repositories;

namespace ACSDining.Service
{
    public interface IOrderMenuService: IService<OrderMenu>
    {
        //OrdersDTO OrdersDtoByWeekYear(int week, int year);
        //OrderMenu GetOrderMenuByWeekYear(int week, int year);
        //double SummaryPriceUserByWeekMenu(UserOrdersDTO usorder, MenuForWeek weekmenu);
        double[] UserWeekOrderDishes( List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw);
        double[] UserWeekOrderPaiments(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw);
        //EmployeeOrderDto EmployeeOrderByWeekYear(WeekMenuDto weekmodel, string userid, int numweek, int year);
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

        //public OrdersDTO OrdersDtoByWeekYear(int week, int year)
        //{
        //    return _repository.GetOrdersDtoByWeekYear(week, year);
        //}

        //public OrderMenu GetOrderMenuByWeekYear(int week, int year)
        //{
        //    return _repository.OrderMenuByWeekYear(week, year);
        //}

        //public double SummaryPriceUserByWeekMenu(UserOrdersDTO usorder, MenuForWeek weekmenu)
        //{
        //    return _repository.GetSummaryPriceUserByWeekMenu(usorder, weekmenu);
        //}

        public double[] UserWeekOrderDishes(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
        {
            return _repository.GetUserWeekOrderDishes(quaList,categories,mfw);
        }

        public double[] UserWeekOrderPaiments(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
        {
            return _repository.GetUserWeekOrderPaiments(quaList, categories, mfw);
        }
        
        //public EmployeeOrderDto EmployeeOrderByWeekYear(WeekMenuDto weekmodel, string userid, int numweek, int year)
        //{
        //    return _repository.EmployeeOrderByWeekYear(weekmodel, userid, numweek, year);
        //}

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
