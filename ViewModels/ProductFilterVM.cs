namespace ECommerce521.ViewModels
{
    public record ProductFilterVM (string productName, decimal? minPrice, decimal? maxPrice, int? categoryId, int? brandId, bool isHot);
}
