using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IOrderMenuService : IService<WeekOrderMenu>
    {
        //Возвращает массив, хранящий количества каждого блюда, заказанного каждый день на неделе всеми клиентами
        void UpdateOrderMenu(WeekOrderMenu weekOrder);
        WeekOrderMenu Find(int orderid);
        WeekOrderMenu FindByUserIdWeekYear(string userid, WeekYearDto wyDto);
        List<WeekOrderMenu> GetOrderMenuByWeekYear(WeekYearDto wyDto);
        //Возвращает список пользователей, которые уже сделали заказ на указанное дневное меню для отправки им сообщения
        List<User> GetUsersMadeBooking(int daymenuid);
        int GetCountByWeekYear(WeekYearDto wyDto);
    }

    public class OrderMenuService : Service<WeekOrderMenu>, IOrderMenuService
    {
        private readonly IRepositoryAsync<WeekOrderMenu> _repository;

        public OrderMenuService(IRepositoryAsync<WeekOrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }
        
        public void UpdateOrderMenu(WeekOrderMenu weekOrder)
        {
            _repository.Update(weekOrder);
        }

        public WeekOrderMenu Find(int orderid)
        {
            return _repository.FindOrderMenuById(orderid);
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
                .Include(om => om.DayOrderMenus.Select(dom=>dom.MenuForDay.WorkingDay.DayOfWeek))
                .Select()
                .FirstOrDefault(
                    om =>
                        string.Equals(om.User.Id, userid) &&
                        om.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                        om.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year);
        }
        public List<User> GetUsersMadeBooking(int daymenuid)
        {
            return _repository.GetUsersMadeOrder(daymenuid);
        }

        public int GetCountByWeekYear(WeekYearDto wyDto)
        {
            return _repository.GetCountByWeekYear(wyDto);
        }
    }
}
