using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.HelpClasses
{
    public static class MapHelper
    {
        public static List<DishType> GetDishCategories(IUnitOfWorkAsync unitOfWorkAsync)
        {
            var cats = unitOfWorkAsync.RepositoryAsync<DishType>().GetAll();

            return cats;
        }
        /// <summary>
        /// Получить количество категорий блюд
        /// </summary>
        /// <param name="unitOfWorkAsync"></param>
        /// <returns></returns>
        public static int GetDishCategoriesCount(IUnitOfWorkAsync unitOfWorkAsync)
        {
            var cats = unitOfWorkAsync.RepositoryAsync<DishType>();

            string[] categories = cats.Queryable()
                .Select(dt => dt.Category)
                .AsQueryable()
                .ToArrayAsync().Result;
            return categories.Length;
        }

        /// <summary>
        /// Получить массив категорий блюд
        /// </summary>
        /// <param name="unitOfWorkAsync"></param>
        /// <returns></returns>
        public static string[] GetCategoriesStrings(IUnitOfWorkAsync unitOfWorkAsync)
        {
            var cats = unitOfWorkAsync.RepositoryAsync<DishType>();
            string[] categories = cats.Queryable()
                .Select(dt => dt.Category)
                .AsQueryable()
                .ToArrayAsync().Result;
            return categories;
        }
        /// <summary>
        /// Получить массив категорий блюд
        /// </summary>
        /// <param name="_db"></param>
        /// <returns></returns>
        public static string[] GetCategoriesStrings(ApplicationDbContext _db)
        {
            return _db.DishTypes.Select(dt => dt.Category).ToArray();
        }


        public static DataTable CreateDataTable<T>(IEnumerable<T> ids,string colname)
        {
            DataTable table = new DataTable();
            table.Columns.Add(colname, typeof(T));
            foreach (T val in ids)
            {
                table.Rows.Add(val);
            }
            return table;
        }
    }
}
