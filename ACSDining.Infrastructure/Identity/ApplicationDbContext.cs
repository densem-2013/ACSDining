﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Migrations;

namespace ACSDining.Infrastructure.Identity
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Globalization;
    using System.Collections.Generic;
    using ACSDining.Core.Domains;

    public partial class ApplicationDbContext : IdentityDbContext<User>
    { 
        public ApplicationDbContext()
            : base("name=ApplicationDbContext", throwIfV1Schema: false)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
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
        public virtual DbSet<WeekOrderMenu> WeekOrderMenus { get; set; }
        public virtual DbSet<PlannedWeekOrderMenu> PlannedWeekOrderMenus { get; set; }
        public virtual DbSet<DishQuantity> DishQuantities { get; set; }
        public virtual DbSet<DishDetail> DishDetails { get; set; }
        public virtual DbSet<ACSDining.Core.Domains.DayOfWeek> Days { get; set; }
        public virtual DbSet<Year> Years { get; set; }
        public virtual DbSet<DishQuantityRelations> DQRelations { get; set; }
        public virtual DbSet<FoodQuantityRelations> FQRelations { get; set; }
        public virtual DbSet<Food> Foods { get; set; }
        public virtual DbSet<FoodCategory> FoodCategories { get; set; }
        public virtual DbSet<FoodQuantity> FoodQuantities { get; set; }
        public virtual DbSet<WorkingWeek> WorkingWeeks { get; set; }
        public virtual DbSet<WorkingDay> WorkingDays { get; set; }
        public virtual DbSet<DayOrderMenu> DayOrderMenus { get; set; }
        public virtual DbSet<PlannedDayOrderMenu> PlannedDayOrderMenus { get; set; } 
    }
}
