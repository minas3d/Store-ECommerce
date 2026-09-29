using ECommerce521.Models;
using ECommerce521.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ECommerce521.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private ApplicationDbContext _context = new();

        public IActionResult Index(ProductFilterVM productFilterVM, int page = 1)
        {
            var products = _context.Products.AsNoTracking().Include(e=>e.Category).Include(e=>e.Brand).AsQueryable();

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

            ViewBag.categories = _context.Categories.AsNoTracking().AsEnumerable();
            ViewData["brands"] = _context.Brands.AsNoTracking().AsEnumerable();

            return View(products.AsEnumerable());
        }

        public IActionResult Create()
        {
            var categories = _context.Categories.AsNoTracking().AsEnumerable();
            var brands = _context.Brands.AsNoTracking().AsEnumerable();

            return View(new CategoryWithBrandVM()
            {
                Categories = categories,
                Brands = brands
            });
        }

        [HttpPost]
        public IActionResult Create(Product product, IFormFile Img, List<IFormFile> SubImgs)
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

            var productCreated = _context.Products.Add(product);
            _context.SaveChanges();

            if(SubImgs.Count > 0)
            {
                foreach (var item in SubImgs)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }

                    _context.productSubImages.Add(new()
                    {
                        Img = fileName,
                        //ProductId = productCreated.Entity.Id,
                        ProductId = product.Id,
                    });
                }

                _context.SaveChanges();
            }

            TempData["success-notification"] = "Add Product Successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var categories = _context.Categories.AsNoTracking().AsEnumerable();
            var brands = _context.Brands.AsNoTracking().AsEnumerable();

            var product = _context.Products.AsNoTracking().FirstOrDefault(e => e.Id == id);
            var productSubImages = _context.productSubImages.AsNoTracking().Where(e => e.ProductId == id).AsEnumerable();

            return View(new CategoryWithBrandVM()
            {
                Categories = categories,
                Brands = brands,
                Product = product, 
                ProductSubImages = productSubImages
            });
        }

        [HttpPost]
        public IActionResult Edit(Product product, IFormFile? Img, List<IFormFile>? SubImgs)
        {
            var productInDb = _context.Products.AsNoTracking().FirstOrDefault(e => e.Id == product.Id);

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

            _context.Products.Update(product);
            _context.SaveChanges();

            if (SubImgs is not null && SubImgs.Count > 0)
            {
                var productSubImages = _context.productSubImages.AsNoTracking().Where(e => e.ProductId == product.Id).AsEnumerable();

                foreach (var item in SubImgs)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }

                    _context.productSubImages.Add(new()
                    {
                        Img = fileName,
                        //ProductId = productCreated.Entity.Id,
                        ProductId = product.Id,
                    });
                }

                foreach (var item in productSubImages)
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", item.Img);

                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);

                    _context.productSubImages.Remove(item);
                }

                _context.SaveChanges();
            }

            TempData["success-notification"] = "Update Product Successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteSubImage(int id, string img) 
        {
            var subImg = _context.productSubImages.FirstOrDefault(e => e.ProductId == id && e.Img == img);

            if (subImg is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), "Home", new { area = "Admin" });

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", subImg.Img);

            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            _context.Remove(subImg);
            _context.SaveChanges();

            return RedirectToAction(nameof(Edit), new { id = id });
        }

        [HttpPost]
        public IActionResult AddSubImg(int productId, List<IFormFile> SubNewImgs)
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

                    _context.productSubImages.Add(new()
                    {
                        Img = fileName,
                        //ProductId = productCreated.Entity.Id,
                        ProductId = productId,
                    });
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        public IActionResult Delete(int id)
        {
            var product = _context.Products.FirstOrDefault(e => e.Id == id);

            if (product is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), "Home", new { area = "Admin" });

            // Delete Old Img from wwwroot
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", product.MainImg);

            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            var productSubImages = _context.productSubImages.AsNoTracking().Where(e => e.ProductId == id).AsEnumerable();

            foreach (var item in productSubImages)
            {
                var oldSubImgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\sub_images", item.Img);

                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                _context.productSubImages.Remove(item);
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            TempData["success-notification"] = "Delete Product Successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
