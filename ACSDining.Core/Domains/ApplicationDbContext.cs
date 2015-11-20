﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Linq;

namespace ACSDining.Core.Domains
{
    using ACSDining.Core.Migrations;
    using ACSDining.Core.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Globalization;
    using System.Collections.Generic;

    public partial class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext()
            : base("name=ApplicationDbContext", throwIfV1Schema: false)
        {
        }

        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<MenuForDay>()
                .HasMany(mfd => mfd.Dishes).WithMany(m => m.MenusForDay)
                .Map(t => t.MapLeftKey("MenuID")
                    .MapRightKey("DishID")
                    .ToTable("MFD_Dishes"));


            base.OnModelCreating(modelBuilder);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public virtual DbSet<Dish> Dishes { get; set; }
        public virtual DbSet<DishType> DishTypes { get; set; }
        public virtual DbSet<MenuForDay> MenuForDays { get; set; }
        public virtual DbSet<MenuForWeek> MenuForWeeks { get; set; }
        public virtual DbSet<OrderMenu> OrderMenus { get; set; }
        public virtual DbSet<PlannedOrderMenu> PlannedOrderMenus { get; set; }
        public virtual DbSet<DishQuantity> DishQuantities { get; set; }
        public virtual DbSet<DishDetail> DishDetails { get; set; }
        public virtual DbSet<DayOfWeek> Days { get; set; }
        public virtual DbSet<Year> Years { get; set; }

        public Func<int> CurrentWeek = () =>
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
            OrderMenu order = OrderMenus.Find(orderid);
            int menuforweekid = order.MenuForWeek.ID;
            List<DishQuantity> quaList =
                DishQuantities.Where(q => q.OrderMenuID == orderid && q.MenuForWeekID == menuforweekid)
                    .ToList();

            string[] categories = DishTypes.OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            for (int i = 1; i <= 5; i++)
            {
                for (int j = 1; j <= categories.Length; j++)
                {
                    var firstOrDefault = quaList.FirstOrDefault(
                        q => q.DayOfWeekID == i && q.DishTypeID == j
                        );
                    if (firstOrDefault != null)
                        dquantities[(i - 1)*4 + j - 1] = firstOrDefault.Quantity;
                }
            }
            return dquantities;
        }

        public double[] GetUserWeekOrderPaiments(int orderid)
        {
            double[] paiments = new double[20];
            double[] unitprices = new double[20];
            OrderMenu order = OrderMenus.Find(orderid);
            int menuforweekid = order.MenuForWeek.ID;
            List<DishQuantity> quaList =
                DishQuantities.Where(q => q.OrderMenuID == orderid && q.MenuForWeekID == menuforweekid)
                    .ToList();

            string[] categories = DishTypes.OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            MenuForWeek mfw = MenuForWeeks.Find(menuforweekid);
            for (int i = 1; i <= 5; i++)
            {
                MenuForDay daymenu = mfw.MenuForDay.ElementAt(i - 1);
                for (int j = 1; j <= categories.Length; j++)
                {
                    var firstOrDefault = quaList.FirstOrDefault(
                        q => q.DayOfWeekID == i && q.DishTypeID == j
                        );
                    if (firstOrDefault != null)
                        paiments[(i - 1)*4 + j - 1] = firstOrDefault.Quantity * daymenu.Dishes.ElementAt(j - 1).Price;
                }
            }
            return paiments;
        }

        public int GetNextWeekOfYear()
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
    }
}
