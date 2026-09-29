namespace ECommerce521.Models
{
    public class ProductColor
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;
        public string Color { get; set; } = string.Empty;
    }
}
