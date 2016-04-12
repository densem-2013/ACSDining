using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IOrderMenuService : IService<WeekOrderMenu>
    {
        //double[] UserWeekOrderDishes(List<DishQuantityRelations> quaList, int catLength, MenuForWeek mfw);
        //получить стоимости блюд, заказанные пользователем за неделю
        //double[] UserWeekOrderPaiments(List<DishQuantityRelations> quaList, int catLength, MenuForWeek mfw);
        IQueryable<WeekOrderMenu> GetAllByWeekYear(WeekYearDto wyDto);
        void UpdateOrderMenu(WeekOrderMenu weekOrder);
        WeekOrderMenu Find(int orderid);
        WeekOrderMenu FindByUserIdWeekYear(string userid, WeekYearDto wyDto);
        List<WeekOrderMenu> GetOrderMenuByWeekYear(WeekYearDto wyDto);
    }

    public class OrderMenuService : Service<WeekOrderMenu>, IOrderMenuService
    {
        private readonly IRepositoryAsync<WeekOrderMenu> _repository;

        public OrderMenuService(IRepositoryAsync<WeekOrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }


        //public double[] UserWeekOrderDishes(List<DishQuantityRelations> quaList, int catLength, MenuForWeek mfw)
        //{
        //    return _repository.GetUserWeekOrderDishes(quaList, catLength, mfw);
        //}

        //public double[] UserWeekOrderPaiments(List<DishQuantityRelations> quaList, int catLength, MenuForWeek mfw)
        //{
        //    return _repository.GetUserWeekOrderPaiments(quaList, catLength, mfw);
        //}

        public IQueryable<WeekOrderMenu> GetAllByWeekYear(WeekYearDto wyDto)
        {
            return _repository.Query().Include(om => om.MenuForWeek.WorkingWeek.Year).Select().Where(
                om =>
                    om.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                    om.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year).AsQueryable();
        }

        public void UpdateOrderMenu(WeekOrderMenu weekOrder)
        {
            _repository.Update(weekOrder);
        }

        public WeekOrderMenu Find(int orderid)
        {
            return _repository.Find(orderid);
        }

        public List<WeekOrderMenu> GetOrderMenuByWeekYear(WeekYearDto wyDto)
        {
            return _repository.OrdersMenuByWeekYear(wyDto);
        }

        public WeekOrderMenu FindByUserIdWeekYear(string userid, WeekYearDto wyDto)
        {
            return _repository.Query()
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(om => om.User)
                .Select()
                .FirstOrDefault(
                    om =>
                        string.Equals(om.User.Id, userid) &&
                        om.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                        om.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year);
        }
    }
}
