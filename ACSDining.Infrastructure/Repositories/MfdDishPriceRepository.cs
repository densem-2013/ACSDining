﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Identity;
using LinqKit;

namespace ACSDining.Infrastructure.Repositories
{
    public static class MfdDishPriceRepository
    {
        public static List<DishModelDto> DishesByCategory(this IRepositoryAsync<MfdDishPriceRelations> repository,
            string category, int? menufordayid = null)
        {
            //if (menufordayid == null)
            //{
            //    menufordayid = repository.GetRepositoryAsync<MenuForWeek>().GetCurrentMenuForDay().ID;
            //}
            return
                repository.GetRepositoryAsync<Dish>().Query()
                .Include(d => d.DishDetail)
                .Include(d => d.DishType)
                .Include(d => d.CurrentPrice)
                .Include(d => d.MfdDishPriceRelations)
                .Select()
                    .Where(d => string.Equals(d.DishType.Category, category))
                    .Select(d => repository.GetDishModelDto(d, menufordayid))
                    .ToList();
        }

        public static DishModelDto GetDishModelDto(this IRepositoryAsync<MfdDishPriceRelations> repository, Dish dish,
            int? menufordayid = null)
        {
            DishModelDto dto = null;
            double price = 0.00;
            if (menufordayid == null)
            {
                price = dish.CurrentPrice.Price;
            }
            else
            {
                MfdDishPriceRelations first =
                    repository.Query()
                        .Include(mp => mp.DishPrice)
                        .Select()
                        .FirstOrDefault(mfdpr => mfdpr.DishId == dish.DishID && mfdpr.MenuForDayId == menufordayid);
                if (first != null)
                {
                    price = first.DishPrice.Price;
                }
            }
            dto = new DishModelDto
            {
                DishId = dish.DishID,
                Title = dish.Title ?? "Блюдо не выбрано",
                ProductImage = dish.ProductImage,
                Price = price,
                Category = dish.DishType.Category,
                Description = dish.Description
            };
            return dto;
        }

        public static MenuForDayDto GetMenuForDayDto(this IRepositoryAsync<MfdDishPriceRelations> repository,
            MenuForDay mfd)
        {
            List<MfdDishPriceRelations> mfdrels = repository.Queryable().Where(mp => mp.MenuForDayId == mfd.ID).ToList();
            List<Dish> dishes =
                repository.GetRepositoryAsync<Dish>()
                    .Queryable().Include("DishType").Include("DishDetail")
                    .Where(d => mfdrels.Select(md => md.DishId).Contains(d.DishID)).OrderBy(d => d.DishType.Id)
                    .ToList();
            List<DishModelDto> dishmodels = dishes.Select(d => repository.GetDishModelDto(d, mfd.ID)).ToList();
            return new MenuForDayDto
            {
                Id = mfd.ID,
                TotalPrice = mfd.TotalPrice,
                Dishes = dishmodels,
                OrderCanBeChanged = mfd.OrderCanBeChanged,
                OrderWasBooking = repository.GetRepositoryAsync<WeekOrderMenu>().GetUsersMadeOrder(mfd.ID).Any()
            };
        }

        public static WeekMenuDto GetWeekMenuDto(this IRepositoryAsync<MfdDishPriceRelations> repository,
            MenuForWeek mfw)
        {
            WeekYearDto wyDto = WeekYearDto.MapDto(mfw.WorkingWeek);
            return new WeekMenuDto
            {
                Id = mfw.ID,
                SummaryPrice = mfw.SummaryPrice,
                MfdModels = mfw.MenuForDay.Select(repository.GetMenuForDayDto).ToList(),
                OrderCanBeCreated = mfw.OrderCanBeCreated,
                WorkWeekDays = mfw.WorkingWeek.WorkingDays.Select(wd => wd.IsWorking).ToArray(),
                WorkingDaysAreSelected = mfw.WorkingDaysAreSelected,
                DayNames = repository.Context.GetDayNames(wyDto).Result,
                WeekYear = wyDto
            };
        }

        public static DishPrice GetDishPrice(this IRepositoryAsync<MfdDishPriceRelations> repository, double price)
        {
            DishPrice dishPrice =
                repository.GetRepositoryAsync<DishPrice>()
                    .Queryable()
                    .FirstOrDefault(dp => Math.Abs(dp.Price - price) < 0.001);
            if (dishPrice == null)
            {
                dishPrice = new DishPrice {Price = price};
                repository.Context.Entry(dishPrice).State = EntityState.Added;
            }
            return dishPrice;
        }

        public static void UpdateDish(this IRepositoryAsync<MfdDishPriceRelations> repository, DishModelDto dish)
        {
            Dish updish = repository.GetRepositoryAsync<Dish>().Find(dish.DishId);
            updish.Description = dish.Description;
            updish.Title = dish.Title;
            updish.CurrentPrice = repository.GetDishPrice(dish.Price);
            repository.Context.Entry(updish).State = EntityState.Modified;
        }

        public static void DeleteDish(this IRepositoryAsync<MfdDishPriceRelations> repository, int dishid)
        {
            Dish updish = repository.GetRepositoryAsync<Dish>().Find(dishid);
            updish.Deleted = true;
            repository.Context.Entry(updish).State = EntityState.Modified;
        }

        public static void InsertDish(this IRepositoryAsync<MfdDishPriceRelations> repository, DishModelDto dishmodel)
        {
            Dish newdish = new Dish
            {
                Title = dishmodel.Title,
                CurrentPrice = repository.GetDishPrice(dishmodel.Price),
                Description = dishmodel.Description,
                ProductImage = dishmodel.ProductImage,
                DishType =
                    repository.GetRepositoryAsync<DishType>()
                        .Queryable()
                        .FirstOrDefault(dt => string.Equals(dt.Category, dishmodel.Category))
            };
            repository.Context.Entry(newdish).State = EntityState.Added;
        }

        public static void UpdateMfdDishes(this IRepositoryAsync<MfdDishPriceRelations> repository, MenuForDayDto menuforday)
        {

            MenuForDay menuFd = repository.GetRepositoryAsync<MenuForDay>().Find(menuforday.Id);

            int[] dishidarray = menuforday.Dishes.Select(d => d.DishId).ToArray();
            menuFd.DishPriceMfdRelations = repository.Queryable().Where(mdp => mdp.MenuForDayId == menuforday.Id).ToList();
            for (int i = 0; i < dishidarray.Length; i++)
            {
                MfdDishPriceRelations rel=menuFd.DishPriceMfdRelations.ElementAt(i);
                rel.DishId = dishidarray[i];
                rel.DishPrice = repository.GetDishPrice(menuforday.Dishes.ElementAt(i).Price);
                repository.Context.Entry(rel).State=EntityState.Modified;
            }
            menuFd.TotalPrice = menuforday.TotalPrice;

        }
    }
}
