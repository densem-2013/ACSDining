using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Repository.Repositories;

namespace ACSDining.Service
{
    public interface IOrderMenuService : IService<WeekOrderMenu>
    {
        double[] UserWeekOrderDishes(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw);
        //получить стоимости блюд, заказанные пользователем за неделю
        double[] UserWeekOrderPaiments(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw);
        IQueryable<WeekOrderMenu> GetAllByWeekYear(int numweek, int year);
        void UpdateOrderMenu(WeekOrderMenu weekOrder);
        WeekOrderMenu Find(int orderid);
        WeekOrderMenu FindByUserIdWeekYear(string userid, int week, int year);
        List<WeekOrderMenu> GetOrderMenuByWeekYear(int week, int year);
    }

    public class OrderMenuService : Service<WeekOrderMenu>, IOrderMenuService
    {
        private readonly IRepositoryAsync<WeekOrderMenu> _repository;

        public OrderMenuService(IRepositoryAsync<WeekOrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }


        public double[] UserWeekOrderDishes(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
        {
            return _repository.GetUserWeekOrderDishes(quaList, categories, mfw);
        }

        public double[] UserWeekOrderPaiments(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
        {
            return _repository.GetUserWeekOrderPaiments(quaList, categories, mfw);
        }

        public IQueryable<WeekOrderMenu> GetAllByWeekYear(int numweek, int year)
        {
            return _repository.Query().Include(om => om.MenuForWeek.WorkingWeek.Year).Select().Where(
                om =>
                    om.MenuForWeek.WorkingWeek.WeekNumber == numweek &&
                    om.MenuForWeek.WorkingWeek.Year.YearNumber == year).AsQueryable();
        }

        public void UpdateOrderMenu(WeekOrderMenu weekOrder)
        {
            _repository.Update(weekOrder);
        }

        public WeekOrderMenu Find(int orderid)
        {
            return _repository.Find(orderid);
        }

        public List<WeekOrderMenu> GetOrderMenuByWeekYear(int week, int year)
        {
            return _repository.OrdersMenuByWeekYear(week, year);
        }

        public WeekOrderMenu FindByUserIdWeekYear(string userid, int week, int year)
        {
            return _repository.Query()
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(om => om.User)
                .Select()
                .FirstOrDefault(
                    om =>
                        string.Equals(om.User.Id, userid) &&
                        om.MenuForWeek.WorkingWeek.WeekNumber == week &&
                        om.MenuForWeek.WorkingWeek.Year.YearNumber == year);
        }
    }
}
