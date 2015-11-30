using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _acsContext;
        private Hashtable _repositories;

        public UnitOfWork()
        {
            _acsContext = new ApplicationDbContext();
        }

        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(Repository<>);

                var repositoryInstance =
                    Activator.CreateInstance(repositoryType
                            .MakeGenericType(typeof(T)), _acsContext);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }

        public void SubmitChanges()
        {
            _acsContext.SaveChanges();
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _acsContext.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }     
        //#region Context Members

        public static Func<int> CurrentWeek = () =>
        {
            CultureInfo myCI = new CultureInfo("uk-UA");
            Calendar myCal = myCI.Calendar;

            // Gets the DTFI properties required by GetWeekOfYear.
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            System.DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
            DateTime CurDay = new System.DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            return myCal.GetWeekOfYear(CurDay, myCWR, myFirstDOW);
        };

        public double[] GetUserWeekOrderDishes(int orderid)
        {
            double[] dquantities = new double[20];
            OrderMenu order = _acsContext.OrderMenus.Find(orderid);
            int menuforweekid = order.MenuForWeek.ID;
            List<DishQuantityRelations> quaList =
                _acsContext.DQRelations.Where(dqr => dqr.OrderMenuID == orderid && dqr.MenuForWeekID == menuforweekid)
                    .ToList();

            string[] categories = _acsContext.DishTypes.OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            for (int i = 1; i <= 5; i++)
            {
                for (int j = 1; j <= categories.Length; j++)
                {
                    var firstOrDefault = quaList.FirstOrDefault(
                        q => q.DayOfWeekID == i && q.DishTypeID == j
                        );
                    if (firstOrDefault != null)
                        dquantities[(i - 1) * 4 + j - 1] = firstOrDefault.DishQuantity.Quantity;
                }
            }
            return dquantities;
        }

        public double[] GetUserWeekOrderPaiments(int orderid)
        {
            double[] paiments = new double[20];
            double[] unitprices = new double[20];
            OrderMenu order = _acsContext.OrderMenus.Find(orderid);
            int menuforweekid = order.MenuForWeek.ID;
            List<DishQuantityRelations> quaList =
                _acsContext.DQRelations.Where(dqr => dqr.OrderMenuID == orderid && dqr.MenuForWeekID == menuforweekid)
                    .ToList();

            string[] categories = _acsContext.DishTypes.OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            MenuForWeek mfw = _acsContext.MenuForWeeks.Find(menuforweekid);
            for (int i = 1; i <= 5; i++)
            {
                MenuForDay daymenu = mfw.MenuForDay.ElementAt(i - 1);
                for (int j = 1; j <= categories.Length; j++)
                {
                    var firstOrDefault = quaList.FirstOrDefault(
                        q => q.DayOfWeekID == i && q.DishTypeID == j
                        );
                    if (firstOrDefault != null)
                        paiments[(i - 1) * 4 + j - 1] = firstOrDefault.DishQuantity.Quantity * daymenu.Dishes.ElementAt(j - 1).Price;
                }
            }
            return paiments;
        }

        public double[] GetUnitWeekPrices(int menuforweekid)
        {
            double[] unitprices = new double[20];

            string[] categories = _acsContext.DishTypes.OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            MenuForWeek mfw = _acsContext.MenuForWeeks.Find(menuforweekid);
            for (int i = 0; i < 5; i++)
            {
                MenuForDay daymenu = mfw.MenuForDay.ElementAt(i);
                for (int j = 0; j < categories.Length; j++)
                {
                    unitprices[i * 4 + j] = daymenu.Dishes.ElementAt(j).Price;
                }
            }
            return unitprices;
        }
        public int GetNextWeekYear()
        {
            int curweek = CurrentWeek();
            if (curweek >= 52)
            {
                DateTime LastDay = new System.DateTime(DateTime.Now.Year, 12, 31);
                if (LastDay.DayOfWeek < System.DayOfWeek.Thursday || curweek == 53)
                {
                    return 1;
                }
            }

            return curweek + 1;
        }
        //#endregion
    }
}
