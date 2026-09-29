using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Mapster;

namespace ECommerce521.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;// = new();

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);

            //var applicationUser = registerVM.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(new()
            {
                Name = registerVM.Name,
                Address = registerVM.Address,
                Email = registerVM.Email,
                UserName = registerVM.UserName,
            }, registerVM.Password);

            if(!result.Succeeded)
            {
                foreach (var item in result.Errors)
                    ModelState.AddModelError(string.Empty, item.Code);

                return View(registerVM);
            }

            // Send Email Confirmation

            TempData["success-notification"] = "Create Account Successfully, Please Confirm Your Email!";
            return RedirectToAction("Login");
        }
    }
}
