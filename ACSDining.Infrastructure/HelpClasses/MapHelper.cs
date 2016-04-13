using System.Data.Entity;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.HelpClasses
{
    public static class MapHelper
    {
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
        public static string[] GetCategories(IUnitOfWorkAsync unitOfWorkAsync)
        {
            var cats = unitOfWorkAsync.RepositoryAsync<DishType>();

            string[] categories = cats.Queryable()
                .Select(dt => dt.Category)
                .AsQueryable()
                .ToArrayAsync().Result;
            return categories;
        }
    }
}
