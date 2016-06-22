using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.Repositories
{
    public static class OrderMenuRepository
    {
      public static List<WeekOrderMenu> OrdersMenuByWeekYear(this IRepositoryAsync<WeekOrderMenu> repository, WeekYearDto wyDto)
        {
            List<WeekOrderMenu> pagedOrders = repository.Query()
                .Include(om => om.User)
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                .Select().Where(
                    om =>
                        om.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                        om.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year)
                .OrderBy(wp => wp.User.LastName)
                .ToList();
            
            return pagedOrders;
        }
      public static void PrevOrdersMenuById(this IRepositoryAsync<WeekOrderMenu> repository, int weekordid)
      {
          WeekOrderMenu curOrderMenu = repository.FindOrderMenuById(weekordid);
          WeekYearDto curwyDto = new WeekYearDto
          {
              Week = curOrderMenu.MenuForWeek.WorkingWeek.WeekNumber,
              Year = curOrderMenu.MenuForWeek.WorkingWeek.Year.YearNumber
          };
          WeekYearDto preWeekYearDto = YearWeekHelp.GetPrevWeekYear(curwyDto);


          WeekOrderMenu prevWeekOrder = repository.Query()
              .Include(om => om.User)
              .Include(om => om.MenuForWeek.WorkingWeek.Year)
              .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
              .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
              .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
              .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
              .Select().FirstOrDefault(om =>
                      string.Equals(om.User.Id,curOrderMenu.User.Id) &&
                      om.MenuForWeek.WorkingWeek.WeekNumber == preWeekYearDto.Week &&
                      om.MenuForWeek.WorkingWeek.Year.YearNumber == preWeekYearDto.Year);
          if (prevWeekOrder == null) return ;
          List<DayOrderMenu> curDayOrderMenus = repository.DayOrdsByWeekOrdId(weekordid);
          List<DayOrderMenu> prevDayOrderMenus = repository.DayOrdsByWeekOrdId(prevWeekOrder.Id);
          curDayOrderMenus.ForEach(curdayord =>
          {
              if (curdayord.MenuForDay.WorkingDay.IsWorking && curdayord.MenuForDay.OrderCanBeChanged)
              {
                  DayOrderMenu prevDayOrd =
                      prevDayOrderMenus.FirstOrDefault(dord => dord.MenuForDay.WorkingDay.DayOfWeek.Id == curdayord.MenuForDay.WorkingDay.DayOfWeek.Id);
                  List<DishQuantityRelations> curdrels =
                      repository.GetRepositoryAsync<DishQuantityRelations>().GetRelationsListByDayOrdId(curdayord.Id);
                  List<DishQuantityRelations> prevdrels =
                      repository.GetRepositoryAsync<DishQuantityRelations>().GetRelationsListByDayOrdId(prevDayOrd.Id);
                  for (int i = 0; i < curdrels.Count; i++)
                  {
                      DishQuantityRelations rel = curdrels[i];
                      rel.DishQuantityId = prevdrels.Count == 0
                          ? repository.GetRepositoryAsync<MfdDishPriceRelations>().GetDishPrice(0.00).Id
                          : prevdrels[i].DishQuantityId;
                      repository.Context.Entry(rel).State=EntityState.Modified;
                  }
              }
          });
      }
      public static SetAsPrevDto GetSetAsPrevDtoById(this IRepositoryAsync<WeekOrderMenu> repository, int weekordid)
      {
          WeekOrderMenu curOrderMenu = repository.FindOrderMenuById(weekordid);
          WeekYearDto curwyDto = new WeekYearDto
          {
              Week = curOrderMenu.MenuForWeek.WorkingWeek.WeekNumber,
              Year = curOrderMenu.MenuForWeek.WorkingWeek.Year.YearNumber
          };
          WeekYearDto preWeekYearDto = YearWeekHelp.GetPrevWeekYear(curwyDto);

          WeekOrderMenu prevWeekOrder = repository.Query()
              .Include(om => om.User)
              .Include(om => om.MenuForWeek.WorkingWeek.Year)
              .Select().FirstOrDefault(om =>
                      string.Equals(om.User.Id, curOrderMenu.User.Id) &&
                      om.MenuForWeek.WorkingWeek.WeekNumber == preWeekYearDto.Week &&
                      om.MenuForWeek.WorkingWeek.Year.YearNumber == preWeekYearDto.Year);
          if (prevWeekOrder != null)
              return new SetAsPrevDto
              {
                  PrevWeekOrdId = prevWeekOrder.Id,
                  DayNames = repository.Context.GetDayNames(preWeekYearDto,true).Result,
                  Prevquants = repository.Context.FactDishQuantByWeekOrderId(prevWeekOrder.Id).Result
              };
          return null;
      }
        public static WeekOrderMenu FindOrderMenuById(this IRepositoryAsync<WeekOrderMenu> repository, int orderid)
        {
            return repository.Query()
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(om => om.User)
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                .Include(om => om.DayOrderMenus.Select(dom => dom.MenuForDay.WorkingDay.DayOfWeek))
                .Select()
                .FirstOrDefault(om => om.Id == orderid);
        }


        public static List<User> GetUsersMadeOrder(this IRepositoryAsync<WeekOrderMenu> repository, int daymenuid)
        {
            List<WeekOrderMenu> whohasweekorder = repository.Query()
                .Include(om => om.User)
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                .Select().Where(
                    om => om.DayOrderMenus.Select(dom => dom.MenuForDay.ID).Contains(daymenuid)).ToList();

            return whohasweekorder.Select(wom => wom.User).Distinct().ToList();
        }

        //Получаем список недельных заказов, которые были сделаны на неделе, включающей данное дневное меню
        public static List<WeekOrderMenu> GetWeekUsersOrdByDayMenuId(this IRepositoryAsync<WeekOrderMenu> repository, int daymenuid)
        {
            return repository.Query()
                .Include(om => om.User)
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                .Select().Where(
                    om => om.DayOrderMenus.Select(dom => dom.MenuForDay.ID).Contains(daymenuid)).ToList();
        }

        public static int GetCountByWeekYear(this IRepositoryAsync<WeekOrderMenu> repository, WeekYearDto wyDto)
        {
            return repository.Query()
                .Include(om => om.MenuForWeek.WorkingWeek.Year).Select().Count(
                    om =>
                        om.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                        om.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year);
        }

        public static List<DayOrderMenu> DayOrdsByMenuForDay(this IRepositoryAsync<WeekOrderMenu> repository, int menuid)
        {
            return
                repository.GetRepositoryAsync<DayOrderMenu>()
                    .Query()
                    .Include(dord => dord.WeekOrderMenu.MenuForWeek)
                    .Include(dord => dord.WeekOrderMenu.DayOrderMenus.Select(dom => dom.MenuForDay))
                    .Select()
                    .Where(dom => dom.MenuForDay.ID == menuid)
                    .ToList();
        }
        public static List<DayOrderMenu> DayOrdsByWeekOrdId(this IRepositoryAsync<WeekOrderMenu> repository, int weekordid)
        {
            return
                repository.GetRepositoryAsync<DayOrderMenu>()
                    .Query()
                    .Include(dord => dord.WeekOrderMenu.MenuForWeek)
                    .Include(dord => dord.WeekOrderMenu.DayOrderMenus.Select(dom => dom.MenuForDay))
                    .Select()
                    .Where(dom => dom.WeekOrderMenu.Id == weekordid)
                    .ToList();
        } 
    }
}
