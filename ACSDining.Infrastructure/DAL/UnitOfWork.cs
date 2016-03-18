using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACSDining.Core.DataContext;
using ACSDining.Core.Domains;
using ACSDining.Core.Infrastructure;
using ACSDining.Core.Repositories;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Identity;
using Microsoft.Practices.ServiceLocation;
using DayOfWeek = System.DayOfWeek;

namespace ACSDining.Infrastructure.DAL
{
    public class UnitOfWork : IUnitOfWorkAsync
    {
        #region Private Fields

        private IDataContextAsync _dataContext;
        private bool _disposed;
        private ObjectContext _objectContext;
        private DbTransaction _transaction;
        private Dictionary<string, dynamic> _repositories;

        #endregion Private Fields

        #region Constuctor/Dispose

        public UnitOfWork(IDataContextAsync dataContext)
        {
            _dataContext = dataContext;
            _repositories = new Dictionary<string, dynamic>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // free other managed objects that implement
                // IDisposable only

                try
                {
                    if (_objectContext != null && _objectContext.Connection.State == ConnectionState.Open)
                    {
                        _objectContext.Connection.Close();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // do nothing, the objectContext has already been disposed
                }

                if (_dataContext != null)
                {
                    _dataContext.Dispose();
                    _dataContext = null;
                }
            }

            // release any unmanaged objects
            // set the object references to null

            _disposed = true;
        }

        #endregion Constuctor/Dispose

        public int SaveChanges()
        {
            return _dataContext.SaveChanges();
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IObjectState
        {
            if (ServiceLocator.IsLocationProviderSet)
            {
                return ServiceLocator.Current.GetInstance<IRepository<TEntity>>();
            }

            return RepositoryAsync<TEntity>();
        }

        public Task<int> SaveChangesAsync()
        {
            return _dataContext.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dataContext.SaveChangesAsync(cancellationToken);
        }

        public IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : class, IObjectState
        {
            if (ServiceLocator.IsLocationProviderSet)
            {
                return ServiceLocator.Current.GetInstance<IRepositoryAsync<TEntity>>();
            }

            if (_repositories == null)
            {
                _repositories = new Dictionary<string, dynamic>();
            }

            var type = typeof(TEntity).Name;

            if (_repositories.ContainsKey(type))
            {
                return (IRepositoryAsync<TEntity>)_repositories[type];
            }

            var repositoryType = typeof(Repository<>);

            _repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _dataContext, this));

            return _repositories[type];
        }

        #region Unit of Work Transactions

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            _objectContext = ((IObjectContextAdapter)_dataContext).ObjectContext;
            if (_objectContext.Connection.State != ConnectionState.Open)
            {
                _objectContext.Connection.Open();
            }

            _transaction = _objectContext.Connection.BeginTransaction(isolationLevel);
        }

        public bool Commit()
        {
            _transaction.Commit();
            return true;
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _dataContext.SyncObjectsStatePostCommit();
        }

        #endregion
        #region _acsContext Members

        public static Func<int> CurrentWeek = () =>
        {
            CultureInfo myCi = new CultureInfo("uk-UA");
            Calendar myCal = myCi.Calendar;

            // Gets the DTFI properties required by GetWeekOfYear.
            CalendarWeekRule myCwr = myCi.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDow = myCi.DateTimeFormat.FirstDayOfWeek;
            DateTime curDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            return myCal.GetWeekOfYear(curDay, myCwr, myFirstDow);
        };

        //Get Last Week Number of Year
        public static Func<int,int> YearWeekCount = (int year) =>
        {
            CultureInfo myCi = new CultureInfo("uk-UA");
            Calendar myCal = myCi.Calendar;

            // Gets the DTFI properties required by GetWeekOfYear.
            CalendarWeekRule myCwr = myCi.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDow = myCi.DateTimeFormat.FirstDayOfWeek;
            DateTime lastweek = new DateTime(year, 12, 31);
            return myCal.GetWeekOfYear(lastweek, myCwr, myFirstDow);
        };

        public double[] GetUserWeekOrderDishes(int orderid)
        {
            double[] dquantities = new double[20];
            OrderMenu order = _repositories["OrderMenu"].Find(orderid);
            int menuforweekid = order.MenuForWeek.ID;
            List<DishQuantityRelations> quaList =
                _acsContext.DQRelations.Where(dqr => dqr.OrderMenuID == orderid && dqr.MenuForWeekID == menuforweekid)
                    .ToList();

            string[] categories = _acsContext.DishTypes.OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            MenuForWeek mfw = _acsContext.MenuForWeeks.Find(menuforweekid);
            for (int i = 1; i <= 7; i++)
            {
                WorkingDay workday = mfw.WorkingWeek.WorkingDays.FirstOrDefault(wd => wd.DayOfWeek.ID == i);
                if (workday != null && workday.IsWorking)
                {
                    for (int j = 1; j <= categories.Length; j++)
                    {
                        var firstOrDefault = quaList.FirstOrDefault(
                            q => q.WorkDay.DayOfWeek.ID == i && q.DishTypeID == j
                            );
                        if (firstOrDefault != null)
                            dquantities[(i - 1)*4 + j - 1] = firstOrDefault.DishQuantity.Quantity;
                    }
                }
            }
            return dquantities;
        }

        public double[] GetUserWeekOrderPaiments(int orderid)
        {
            double[] paiments = new double[20];
            OrderMenu order = _acsContext.OrderMenus.Find(orderid);
            int menuforweekid = order.MenuForWeek.ID;
            List<DishQuantityRelations> quaList =
                _acsContext.DQRelations.Where(dqr => dqr.OrderMenuID == orderid && dqr.MenuForWeekID == menuforweekid)
                    .ToList();

            string[] categories = _acsContext.DishTypes.OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            MenuForWeek mfw = _acsContext.MenuForWeeks.Find(menuforweekid);
            for (int i = 1; i <= 7; i++)
            {
                WorkingDay workday = mfw.WorkingWeek.WorkingDays.FirstOrDefault(wd => wd.DayOfWeek.ID == i);
                if (workday != null && workday.IsWorking)
                {
                    MenuForDay daymenu = mfw.MenuForDay.ElementAt(i - 1);
                    for (int j = 1; j <= categories.Length; j++)
                    {
                        var firstOrDefault = quaList.FirstOrDefault(
                            q => q.WorkDay.DayOfWeek.ID == i && q.DishTypeID == j
                            );
                        if (firstOrDefault != null)
                            paiments[(i - 1)*4 + j - 1] = firstOrDefault.DishQuantity.Quantity*
                                                          daymenu.Dishes.ElementAt(j - 1).Price;
                    }
                }
            }
            return paiments;
        }

        public double[] GetUnitWeekPrices(int menuforweekid)
        {
            double[] unitprices = new double[20];

            string[] categories = Repository<DishType>().GetAll().Result.OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            MenuForWeek mfw = Repository<MenuForWeek>().Find(mw=>mw.ID==menuforweekid).Result;
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
        public static WeekYearDTO GetNextWeekYear(WeekYearDTO wyDto)
        {
            WeekYearDTO result=new WeekYearDTO();
            if (wyDto.Week >= 52)
            {
                DateTime lastDay = new DateTime(wyDto.Year, 12, 31);
                if (lastDay.DayOfWeek < DayOfWeek.Thursday || wyDto.Week == 53)
                {
                    result.Week = 1;
                    result.Year = wyDto.Year+1;
                }
            }
            else
            {
                result.Week = wyDto.Week + 1;
                result.Year = wyDto.Year;
            }

            return result;
        }
        public static WeekYearDTO GetPrevWeekYear(WeekYearDTO wyDto)
        {
            WeekYearDTO result = new WeekYearDTO();
            if (wyDto.Week == 1)
            {
                result.Week = YearWeekCount(wyDto.Year);
                result.Year = wyDto.Year - 1;
            }
            else
            {
                result.Week = wyDto.Week - 1;
                result.Year = wyDto.Year;
            }

            return result;
        }

        public WeekMenuDto MenuForWeekToDto(MenuForWeek wmenu, bool emptyDishes = false)
        {
            WeekMenuDto dtoModel = new WeekMenuDto
            {
                ID = wmenu.ID,
                WeekNumber = wmenu.WorkingWeek.WeekNumber,
                SummaryPrice = wmenu.SummaryPrice,
                YearNumber = wmenu.WorkingWeek.Year.YearNumber
            };
            if (emptyDishes)
            {
                List<DishType> dtypes = Repository<DishType>().GetAll().Result;
                dtoModel.MFD_models = new List<MenuForDayDto>();
                foreach (MenuForDay mfd in wmenu.MenuForDay)
                {
                    var dmodels = new List<DishModelDto>();
                    for (int i = 0; i < 4; i++)
                    {
                        DishType firstOrDefault = dtypes.FirstOrDefault(dt => dt.Id == i + 1);
                        if (firstOrDefault != null)
                            dmodels.Add(new DishModelDto
                            {
                                DishID = i + 1,
                                Title = "_",
                                Price = 0,
                                Category = firstOrDefault.Category,
                                Foods = "_"
                            });
                    }

                    dtoModel.MFD_models.Add(new MenuForDayDto
                    {
                        ID = mfd.ID,
                        DayOfWeek = mfd.WorkingDay.DayOfWeek.Name,
                        TotalPrice = mfd.TotalPrice,
                        Dishes = dmodels
                    });
                }
            }
            else
            {
                try
                {

                    dtoModel.MFD_models = wmenu.MenuForDay.ToList().Select(MenuForDayDto.MapDto).ToList();
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            return dtoModel;
        }

        public static bool WeekDaysCanBeChanged(WorkingWeek workweek)
        {
            WeekYearDTO curDto = new WeekYearDTO
            {
                Week = CurrentWeek(),
                Year = DateTime.Now.Year
            };
            WeekYearDTO nextDto = GetNextWeekYear(curDto);
            return (workweek.WeekNumber == curDto.Week && workweek.Year.YearNumber == curDto.Year) ||
                   (workweek.WeekNumber == nextDto.Week && workweek.Year.YearNumber == nextDto.Year);
        }
        #endregion
    }
}
