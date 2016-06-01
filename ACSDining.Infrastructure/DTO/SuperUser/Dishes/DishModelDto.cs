namespace ACSDining.Infrastructure.DTO.SuperUser.Dishes
{
    public class DishModelDto
    {
        public int DishId { get; set; }
        public string Title { get; set; }
        public string ProductImage { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }

    }
}