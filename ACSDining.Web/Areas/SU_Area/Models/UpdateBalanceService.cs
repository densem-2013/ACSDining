using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public static class UpdateBalanceService
    {
        public static void UpdateBalanceByDayMenuCanged(int daimenu)
        {
            IUnitOfWorkAsync unitOfWork = DependencyResolver.Current.GetService<IUnitOfWorkAsync>();
            List<WeekOrderMenu> unitList =
                unitOfWork.RepositoryAsync<WeekOrderMenu>().GetWeekUsersOrdByDayMenuId(daimenu);
            unitList.ForEach(uord => unitOfWork.GetContext().UpdateBalanceByWeekOrderId(uord.Id));
        }
    }
}