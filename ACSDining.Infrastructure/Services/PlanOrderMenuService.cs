using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IPlanOrderMenuServece
    {
        List<PlannedWeekOrderMenu> GetPlannedWeekOrdersByWeekYear(WeekYearDto wyDto);
    }
    public class PlanOrderMenuService : Service<PlannedWeekOrderMenu>, IPlanOrderMenuServece
    {
        private readonly IRepositoryAsync<PlannedWeekOrderMenu> _repository;

        public PlanOrderMenuService(IRepositoryAsync<PlannedWeekOrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public List<PlannedWeekOrderMenu> GetPlannedWeekOrdersByWeekYear(WeekYearDto wyDto)
        {
            return _repository.OrdersMenuByWeekYear(wyDto);
        }
    }
}
