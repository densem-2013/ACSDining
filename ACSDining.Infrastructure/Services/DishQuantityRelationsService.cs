using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IDishQuantityRelationsService : IService<DishQuantityRelations>
    {
        //Получить связи на заказанное количество блюд в указанной фактической дневной заявке на указанное меню рабочего дня
        List<DishQuantityRelations> GetByDayOrderMenuForDay(int dayorderid, int menufordayid);
    }

    public class DishQuantityRelationsService : Service<DishQuantityRelations>, IDishQuantityRelationsService
    {
        private readonly IRepositoryAsync<DishQuantityRelations> _repository;

        public DishQuantityRelationsService(IRepositoryAsync<DishQuantityRelations> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public List<DishQuantityRelations> GetByDayOrderMenuForDay(int dayorderid, int menufordayid)
        {
            return _repository.Query()
                .Include(dq => dq.DishQuantity)
                .Include(dq => dq.MenuForDay.WorkingDay.DayOfWeek)
                .Select()
                .Where(dqr => dqr.MenuForDayId == menufordayid && dqr.DayOrderMenuId == dayorderid)
                .ToList();
        }
    }
}
