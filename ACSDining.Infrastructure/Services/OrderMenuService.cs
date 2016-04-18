using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.Services
{
    public interface IOrderMenuService : IService<WeekOrderMenu>
    {
        //Возвращает массив, хранящий количества каждого блюда, заказанного каждый день на неделе
        double[] SummaryWeekDishesOrderQuantities(WeekYearDto wyDto, int catLenth);
        void UpdateOrderMenu(WeekOrderMenu weekOrder);
        WeekOrderMenu Find(int orderid);
        WeekOrderMenu FindByUserIdWeekYear(string userid, WeekYearDto wyDto);
        List<WeekOrderMenu> GetOrderMenuByWeekYear(WeekYearDto wyDto);
        WeekOrderMenu CreateNew( User user, WeekYearDto wyDto);
        int UpdateUserWeekOrder(IUnitOfWorkAsync unitOfWork, UserWeekOrderDto userWeekOrderDto);
    }

    public class OrderMenuService : Service<WeekOrderMenu>, IOrderMenuService
    {
        private readonly IRepositoryAsync<WeekOrderMenu> _repository;

        public OrderMenuService(IRepositoryAsync<WeekOrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public double[] SummaryWeekDishesOrderQuantities(WeekYearDto wyDto, int catLenth)
        {
            return _repository.SummaryDishesQuantities(wyDto,catLenth);
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
        public int UpdateUserWeekOrder(IUnitOfWorkAsync unitOfWork, UserWeekOrderDto userWeekOrderDto)
        {
            return _repository.UserWeekOrderUpdate(unitOfWork, userWeekOrderDto);
        }

        public WeekOrderMenu CreateNew( User user, WeekYearDto wyDto)
        {
            return _repository.CreateWeekOrderMenu( user, wyDto);
        }
    }
}
