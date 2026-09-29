using ECommerce521.Models;
using ECommerce521.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading.Tasks;

namespace ECommerce521.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        //private ApplicationDbContext _context = new();
        private readonly IRepository<Product> _productRepository;//= new Repository<Product>();
        private readonly IProductSubImageRepository _subImageRepository;// = new ProductSubImageRepository();
        private readonly IRepository<Category> _categoryRepository;// = new Repository<Category>();
        private readonly IRepository<Brand> _brandRepository;// = new Repository<Brand>();

        public ProductController(IRepository<Product> productRepository, IProductSubImageRepository subImageRepository, IRepository<Category> categoryRepository, IRepository<Brand> brandRepository)
        {
            _productRepository = productRepository;
            _subImageRepository = subImageRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
        }

        public async Task<IActionResult> Index(ProductFilterVM productFilterVM, int page = 1)
        {
            var products = await _productRepository.GetAsync(includes: [e => e.Category, e => e.Brand], tracked: false);

            #region Filter
            const decimal discount = 50;

            if (productFilterVM.productName is not null)
            {
                products = products.Where(e => e.Name.Contains(productFilterVM.productName));
                ViewBag.productName = productFilterVM.productName;
            }

            if (productFilterVM.minPrice is not null)
            {
                products = products.Where(e => e.Price - e.Discount / 100 * e.Price >= productFilterVM.minPrice);
                ViewBag.minPrice = productFilterVM.minPrice;
            }

            if (productFilterVM.maxPrice is not null)
            {
                products = products.Where(e => e.Price - e.Discount / 100 * e.Price <= productFilterVM.maxPrice);
                ViewBag.maxPrice = productFilterVM.maxPrice;
            }

            if (productFilterVM.categoryId is not null)
            {
                products = products.Where(e => e.CategoryId == productFilterVM.categoryId);
                ViewBag.categoryId = productFilterVM.categoryId;
            }

            if (productFilterVM.brandId is not null)
            {
                products = products.Where(e => e.BrandId == productFilterVM.brandId);
                ViewBag.brandId = productFilterVM.brandId;
            }

            if (productFilterVM.isHot)
            {
                products = products.Where(e => e.Discount > discount);
                ViewBag.isHot = productFilterVM.isHot;
            }

            #endregion

            #region Pagination

            ViewBag.totalPages = Math.Ceiling(products.Count() / 8.0);
            ViewBag.currentPage = page;
            products = products.Skip((page - 1) * 8).Take(8);

            #endregion

            ViewBag.categories = await _categoryRepository.GetAsync(tracked: false);
            ViewData["brands"] = await _brandRepository.GetAsync(tracked: false);

            return View(products.AsEnumerable());
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAsync(tracked: false);
            var brands = await _brandRepository.GetAsync(tracked: false);

            return View(new CategoryWithBrandVM()
            {
                Categories = categories,
                Brands = brands
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile Img, List<IFormFile> SubImgs)
        {
            if (Img is not null && Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Img.FileName);

                // Save Img in wwwroot
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", fileName);

                //if (!System.IO.File.Exists(filePath))
                //    System.IO.File.Create(filePath);

                using (var stream = System.IO.File.Create(filePath))
                {
                    Img.CopyTo(stream);
                }

                // Save Img Name in Db
                product.MainImg = fileName;
            }

            await _productRepository.CreateAsync(product);
            await _productRepository.CommitAsync();

            if(SubImgs.Count > 0)
            {
                List<ProductSubImage> subImgs = new List<ProductSubImage>();

                foreach (var item in SubImgs)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }

                    subImgs.Add(new()
                    {
                        Img = fileName,
                        //ProductId = productCreated.Entity.Id,
                        ProductId = product.Id,
                    });
                }

                await _subImageRepository.CreateRangeAsync(subImgs);
                await _subImageRepository.CommitAsync();
            }

            TempData["success-notification"] = "Add Product Successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var categories = await _categoryRepository.GetAsync(tracked: false);
            var brands = await _brandRepository.GetAsync(tracked: false);

            var product = await _productRepository.GetOneAsync(e => e.Id == id, tracked: false);
            var productSubImages = await _subImageRepository.GetAsync(e => e.ProductId == id, tracked: false);

            return View(new CategoryWithBrandVM()
            {
                Categories = categories,
                Brands = brands,
                Product = product, 
                ProductSubImages = productSubImages
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile? Img, List<IFormFile>? SubImgs)
        {
            var productInDb = await _productRepository.GetOneAsync(e => e.Id == product.Id, tracked: false);

            if(productInDb is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), "Home", new { area = "Admin" });

            if (Img is not null && Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Img.FileName);

                // Save Img in wwwroot
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", fileName);
                
                //if (!System.IO.File.Exists(filePath))
                //    System.IO.File.Create(filePath);

                using (var stream = System.IO.File.Create(filePath))
                {
                    Img.CopyTo(stream);
                }

                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", productInDb.MainImg);

                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                // Save Img Name in Db
                product.MainImg = fileName;
            }
            else
            {
                product.MainImg = productInDb.MainImg;
            }

            _productRepository.Update(product);
            await _productRepository.CommitAsync();

            if (SubImgs is not null && SubImgs.Count > 0)
            {
                var productSubImages = await _subImageRepository.GetAsync(e => e.ProductId == product.Id, tracked: false);

                List<ProductSubImage> subImages = new();
                List<ProductSubImage> removeSubImages = new();

                foreach (var item in SubImgs)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }

                    subImages.Add(new()
                    {
                        Img = fileName,
                        //ProductId = productCreated.Entity.Id,
                        ProductId = product.Id,
                    });
                }

                await _subImageRepository.CreateRangeAsync(subImages);

                foreach (var item in productSubImages)
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", item.Img);

                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);

                    removeSubImages.Add(item);
                }

                _subImageRepository.DeleteRange(removeSubImages);
                await _subImageRepository.CommitAsync();
            }

            TempData["success-notification"] = "Update Product Successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteSubImage(int id, string img) 
        {
            var subImg = await _subImageRepository.GetOneAsync(e => e.ProductId == id && e.Img == img);

            if (subImg is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), "Home", new { area = "Admin" });

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", subImg.Img);

            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            _subImageRepository.Delete(subImg);
            await _subImageRepository.CommitAsync();

            return RedirectToAction(nameof(Edit), new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> AddSubImg(int productId, List<IFormFile> SubNewImgs)
        {
            if (SubNewImgs.Count > 0)
            {
                foreach (var item in SubNewImgs)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }

                    await _subImageRepository.CreateAsync(new()
                    {
                        Img = fileName,
                        //ProductId = productCreated.Entity.Id,
                        ProductId = productId,
                    });
                }

                await _subImageRepository.CommitAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetOneAsync(e => e.Id == id);

            if (product is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), "Home", new { area = "Admin" });

            // Delete Old Img from wwwroot
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", product.MainImg);

            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            var productSubImages = await _subImageRepository.GetAsync(e => e.ProductId == id, tracked: false);

            foreach (var item in productSubImages)
            {
                var oldSubImgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", item.Img);

                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                _subImageRepository.Delete(item);
            }

            _productRepository.Delete(product);
            await _productRepository.CommitAsync();

            TempData["success-notification"] = "Delete Product Successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
