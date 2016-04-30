using ACSDining.Core.Domains;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class DishModelDto
    {
        public int DishId { get; set; }
        public string Title { get; set; }
        public string ProductImage { get; private set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }

        public static DishModelDto MapDto(Dish dish)
        {
            return new DishModelDto
            {
                DishId = dish.DishID,
                Title = dish.Title,
                ProductImage = dish.ProductImage,
                Price = dish.Price,
                Category = dish.DishType.Category,
                Description = dish.Description
            };
        }
    }
}