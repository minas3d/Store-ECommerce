using ECommerce521.Models;
using ECommerce521.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ECommerce521.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;// = new();

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(ProductFilterVM productFilterVM, int page = 1)
      {
            var products = _context.Products.AsNoTracking().AsQueryable();

            products = products.Include(e => e.Category);

            #region Filter
            const decimal discount = 50;

            if (productFilterVM.productName is not null)
            {
                products = products.Where(e => e.Name.Contains(productFilterVM.productName));
                ViewBag.productName = productFilterVM.productName;
            }

            if(productFilterVM.minPrice is not null)
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

            var categories = _context.Categories.AsNoTracking().AsQueryable();
            var brands = _context.Brands.AsNoTracking().AsQueryable();

            ViewBag.categories = categories.AsEnumerable();
            ViewData["brands"] = brands.AsEnumerable();

            return View(products.AsEnumerable());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public ViewResult Welcome()
        {
            return View();
        }

        public ViewResult PersonalInfo()
        {
            List<Person> persons = [
                new Person() {
                    Id = 1,
                    Name = "Mohamed",
                    Age = 28,
                    Salary = 1000,
                    Skills = ["C#", "SQL", "EF"]
                },
                new Person() {
                    Id = 2,
                    Name = "Ali",
                    Age = 27,
                    Salary = 1500,
                    Skills = ["C++", "JDI+", "MySQL"]
                },
                new Person() {
                    Id = 3,
                    Name = "Mona",
                    Age = 30,
                    Salary = 2000,
                    Skills = ["Python", "SQL"]
                }
            ];

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
