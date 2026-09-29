using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace ECommerce521.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandController : Controller
    {
        private ApplicationDbContext _context = new();

        public IActionResult Index()
        {
            var brands = _context.Brands.AsNoTracking().AsQueryable();

            return View(brands.AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Brand brand, IFormFile file)
        {
            if (file is not null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                // Save Img in wwwroot
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", fileName);

                //if (!System.IO.File.Exists(filePath))
                //    System.IO.File.Create(filePath);

                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(stream);
                }

                // Save Img Name in Db
                brand.Img = fileName;
            }

            _context.Brands.Add(brand);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var brand = _context.Brands.FirstOrDefault(e => e.Id == id);

            if (brand is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), "Home", new { area = "Admin" });

            return View(brand);
        }

        [HttpPost]
        public IActionResult Edit(Brand brand, IFormFile? file)
        {
            var brandInDB = _context.Brands.AsNoTracking().FirstOrDefault(e => e.Id == brand.Id);

            if (brandInDB is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), "Home", new { area = "Admin" });

            if (file is not null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                // Save Img in wwwroot
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", fileName);

                //if (!System.IO.File.Exists(filePath))
                //    System.IO.File.Create(filePath);

                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(stream);
                }

                // Delete Old Img from wwwroot
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", brandInDB.Img);

                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                // Save Img Name in Db
                brand.Img = fileName;
            }
            else
            {
                brand.Img = brandInDB.Img;
            }

            _context.Brands.Update(brand);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var brand = _context.Brands.FirstOrDefault(e => e.Id == id);

            if (brand is null)
                return RedirectToAction(nameof(HomeController.NotFoundPage), "Home", new { area = "Admin" });

            // Delete Old Img from wwwroot
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", brand.Img);

            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            _context.Brands.Remove(brand);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
