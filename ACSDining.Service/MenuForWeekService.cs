using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Repository.Repositories;

namespace ACSDining.Service
{
    public interface IMenuForWeekService: IService<MenuForWeek>
    {
        double[] UnitWeekPrices(int menuforweekid, string[] categories);
        
        MenuForWeek GetWeekMenuByWeekYear(int week, int year);

        List<int> WeekNumbers();

        double SummaryPrice(UserOrdersDTO usorder, int numweek, int year);

        IQueryable<MenuForWeek> GetAll();
        
        Task<bool> DeleteMenuForWeek(int menuid);
    }

    public class MenuForWeekService : Service<MenuForWeek>, IMenuForWeekService
    {
        private readonly IRepositoryAsync<MenuForWeek> _repository;

        public MenuForWeekService(IRepositoryAsync<MenuForWeek> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public double[] UnitWeekPrices(int menuforweekid,string[] categories)
        {
            return _repository.GetUnitWeekPrices(menuforweekid,categories);
        }

        //public MenuForWeek WeekMenuByWeekYear(int week, int year)
        //{
        //    return _repository.GetWeekMenuByWeekYear(week, year);
        //}

        public List<int> WeekNumbers()
        {
            try
            {
                return _repository.GetWeekNumbers();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public double SummaryPrice(UserOrdersDTO usorder, int numweek, int year)
        {
            return _repository.GetSummaryPrice(usorder, numweek, year);
        }

        public IQueryable<MenuForWeek> GetAll()
        {
            return _repository.Queryable();
        }

        public async Task<bool> DeleteMenuForWeek(int menuid)
        {
            return await _repository.DeleteAsync(menuid);
        }

        public MenuForWeek GetWeekMenuByWeekYear(int week, int year)
        {
            return _repository.GetWeekMenuByWeekYear(week, year);
        }
    }
}
