using ACSDining.Core.Domains;

namespace ACSDining.Infrastructure.DTO.Employee
{
    public class OrderDishDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }

        public static OrderDishDto MapDto(Dish dish)
        {
            return new OrderDishDto
            {
                Title = dish.Title,
                Description = dish.Description,
                //Price = dish.Price,
                Category = dish.DishType.Category
            };
        }
    }
}
