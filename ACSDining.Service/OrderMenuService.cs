﻿using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Repository.Repositories;

namespace ACSDining.Service
{
    public interface IOrderMenuService : IService<OrderMenu>
    {
        double[] UserWeekOrderDishes(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw);
        //получить стоимости блюд, заказанные пользователем за неделю
        double[] UserWeekOrderPaiments(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw);
        IQueryable<OrderMenu> GetAllByWeekYear(int numweek, int year);
        void UpdateOrderMenu(OrderMenu order);
        OrderMenu Find(int orderid);
        OrderMenu FindByUserIdWeekYear(string userid, int week, int year);
        List<OrderMenu> GetOrderMenuByWeekYear(int week, int year);
    }

    public class OrderMenuService : Service<OrderMenu>, IOrderMenuService
    {
        private readonly IRepositoryAsync<OrderMenu> _repository;

        public OrderMenuService(IRepositoryAsync<OrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }


        public double[] UserWeekOrderDishes(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
        {
            return _repository.GetUserWeekOrderDishes(quaList, categories, mfw);
        }

        public double[] UserWeekOrderPaiments(List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
        {
            return _repository.GetUserWeekOrderPaiments(quaList, categories, mfw);
        }

        public IQueryable<OrderMenu> GetAllByWeekYear(int numweek, int year)
        {
            return _repository.Query().Include(om => om.MenuForWeek.WorkingWeek.Year).Select().Where(
                om =>
                    om.MenuForWeek.WorkingWeek.WeekNumber == numweek &&
                    om.MenuForWeek.WorkingWeek.Year.YearNumber == year).AsQueryable();
        }

        public void UpdateOrderMenu(OrderMenu order)
        {
            _repository.Update(order);
        }

        public OrderMenu Find(int orderid)
        {
            return _repository.Find(orderid);
        }

        public List<OrderMenu> GetOrderMenuByWeekYear(int week, int year)
        {
            return _repository.OrdersMenuByWeekYear(week, year);
        }

        public OrderMenu FindByUserIdWeekYear(string userid, int week, int year)
        {
            return _repository.Query()
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(om => om.User)
                .Select()
                .FirstOrDefault(
                    om =>
                        string.Equals(om.User.Id, userid) &&
                        om.MenuForWeek.WorkingWeek.WeekNumber == week &&
                        om.MenuForWeek.WorkingWeek.Year.YearNumber == year);
        }
    }
}
