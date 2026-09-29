using System.Threading.Tasks;

namespace ECommerce521.Repositories
{
    public class ProductSubImageRepository : Repository<ProductSubImage>, IProductSubImageRepository
    {
        public ProductSubImageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task CreateRangeAsync(List<ProductSubImage> productSubImages)
        {
            await _context.productSubImages.AddRangeAsync(productSubImages);
        }

        public void DeleteRange(List<ProductSubImage> productSubImages)
        {
            _context.productSubImages.RemoveRange(productSubImages);
        }
    }
}
