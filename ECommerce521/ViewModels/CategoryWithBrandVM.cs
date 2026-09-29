namespace ECommerce521.ViewModels
{
    public class CategoryWithBrandVM
    {
        public IEnumerable<Category> Categories { get; set; } = null!;
        public IEnumerable<Brand> Brands { get; set; } = null!;
        public Product? Product { get; set; }
        public IEnumerable<ProductSubImage> ProductSubImages { get; set; } = null!;
    }
}
