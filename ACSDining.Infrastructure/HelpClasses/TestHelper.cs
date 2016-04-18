using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.HelpClasses
{
    public static class TestHelper
    {
        private static readonly Random Rand = new Random();

        private static List<Dish> GetDishes(IUnitOfWorkAsync _unitOfWork)
        {
            Dish[] dishArray = _unitOfWork.GetContext().Dishes.ToArray();
            string[] categories = MapHelper.GetCategoriesStrings(_unitOfWork);

            Func<string, IEnumerable<Dish>, int> countDish = (str, list) =>
            {
                int coun = list.Count(el => string.Equals(el.DishType.Category, str));
                return coun;
            };
            Dictionary<string, int> catCount = categories.ToDictionary(cat => cat, count => countDish(count, dishArray));
            Func<List<Dish>> getDishes = () =>
            {
                return catCount.Select(pair => dishArray.Where(d => string.Equals(d.DishType.Category, pair.Key)).ElementAt(Rand.Next(pair.Value))).ToList();
            };

            return getDishes();
        }
    }
}
