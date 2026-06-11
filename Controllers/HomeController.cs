using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using furniture_store.Models;
using Microsoft.AspNetCore.Mvc;

namespace furniture_store.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FurnitureContext _context;

        public HomeController(ILogger<HomeController> logger, FurnitureContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            var featuredProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Featured == true)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Categories = categories,
                FeaturedProducts = featuredProducts
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
