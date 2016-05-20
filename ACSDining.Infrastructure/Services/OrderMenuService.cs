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
        //double[] SummaryWeekDishesOrderQuantities(WeekYearDto wyDto, int catLenth);
        void UpdateOrderMenu(WeekOrderMenu weekOrder);
        WeekOrderMenu Find(int orderid);
        WeekOrderMenu FindByUserIdWeekYear(string userid, WeekYearDto wyDto);
        List<WeekOrderMenu> GetOrderMenuByWeekYear(WeekYearDto wyDto, int? pageSize = null, int? page = null);
        //WeekOrderMenu CreateNew( User user, WeekYearDto wyDto);
       // int UpdateUserWeekOrder(int catcount, UserWeekOrderDto userWeekOrderDto);
        //Возвращает список пользователей, которые уже сделали заказ на указанное дневное меню для отправки им сообщения
        List<User> GetUsersMedeBooking(int daymenuid);
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

        //public double[] SummaryWeekDishesOrderQuantities(WeekYearDto wyDto, int catLenth)
        //{
        //    return _repository.SummaryDishesQuantities(wyDto,catLenth);
        //}

        //public double[] GetUserWeekOrderDishes(WeekOrderMenu wom, int daycount, int catlength)
        //{
        //    return _repository.UserWeekOrderDishes(wom, daycount, catlength);
        //}

        public void UpdateOrderMenu(WeekOrderMenu weekOrder)
        {
            _repository.Update(weekOrder);
        }

        public WeekOrderMenu Find(int orderid)
        {
            return _repository.FindOrderMenuById(orderid);
        }

        public List<WeekOrderMenu> GetOrderMenuByWeekYear(WeekYearDto wyDto, int? pageSize = null, int? page = null)
        {
            return _repository.OrdersMenuByWeekYear(wyDto,pageSize,page);
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
        //public int UpdateUserWeekOrder(int catcount, UserWeekOrderDto userWeekOrderDto)
        //{
        //    return _repository.UserWeekOrderUpdate(catcount, userWeekOrderDto);
        //}

        //public WeekOrderMenu CreateNew( User user, WeekYearDto wyDto)
        //{
        //    return _repository.CreateWeekOrderMenu( user, wyDto);
        //}

        public List<User> GetUsersMedeBooking(int daymenuid)
        {
            return _repository.GetUsersMadeOrder(daymenuid);
        }

        public int GetCountByWeekYear(WeekYearDto wyDto)
        {
            return _repository.GetCountByWeekYear(wyDto);
        }
    }
}
