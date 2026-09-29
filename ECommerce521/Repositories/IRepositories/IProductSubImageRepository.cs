using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerce521.Repositories.IRepositories
{
    public interface IProductSubImageRepository : IRepository<ProductSubImage>
    {
        Task CreateRangeAsync(List<ProductSubImage> productSubImages);

        void DeleteRange(List<ProductSubImage> productSubImages);
    }
}
