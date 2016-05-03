using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.HelpClasses
{
    public class OrderCalculationHelp
    {
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        public  Dictionary<int, double[ ,]> WeekOrderDishes { get; set; }

        public OrderCalculationHelp(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWorkAsync = unitOfWork;
            WeekOrderDishes=new Dictionary<int, double[,]>();
        }

        public void AddWeekOrderInfo(int weekid)
        {
            
        }
    }
}
