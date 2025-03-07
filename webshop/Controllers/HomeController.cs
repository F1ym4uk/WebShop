using Microsoft.AspNetCore.Mvc;
using webshop.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using webshop.Models;

namespace webshop.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;


        public HomeController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }


        // Home Catalog
        public async Task<IActionResult> Index(string category, string name, decimal? minPrice, decimal? maxPrice)
        {
            var productsQuery = _context.Products.Where(p => p.StockQuantity > 0).AsQueryable();

            if (!string.IsNullOrEmpty(name))
                productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(name.ToLower()));

            if (!string.IsNullOrEmpty(category))
                productsQuery = productsQuery.Where(p => p.Category.ToLower().Contains(category.ToLower()));

            if (minPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);

            var products = await productsQuery.Take(12).ToListAsync();

            ViewBag.SelectedName = name;
            ViewBag.SelectedCategory = category;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Names = await productsQuery.Select(p => p.Name).Distinct().ToListAsync();
            ViewBag.Categories = await productsQuery.Select(p => p.Category).Distinct().ToListAsync();

            return View(products);
        }



        [HttpGet]
        public async Task<IActionResult> LoadProducts(string category, string name, decimal? minPrice, decimal? maxPrice, int skip, int take)
        {
            var productsQuery = _context.Products.Where(p => p.StockQuantity > 0).AsQueryable();

            if (!string.IsNullOrEmpty(name))
                productsQuery = productsQuery.Where(p => p.Name.Contains(name));

            if (!string.IsNullOrEmpty(category))
                productsQuery = productsQuery.Where(p => p.Category.Contains(category));

            if (minPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);

            var totalProducts = await productsQuery.CountAsync();

            var products = await productsQuery
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(take)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Image
                })
                .ToListAsync();

            // ✅ Получаем корзину из cookies
            var cartItems = GetCartFromCookie();
            var cartProductIds = cartItems.Select(ci => ci.Product.Id).ToList();

            return Json(new
            {
                products,
                hasMore = skip + take < totalProducts,
                isAuthenticated = User.Identity.IsAuthenticated,
                cartProductIds // ✅ Передаем товары, которые есть в корзине
            });
        }

        // ✅ Добавляем метод для получения корзины из cookies
        private List<CartItem> GetCartFromCookie()
        {
            var cartCookie = Request.Cookies["UserCart"];
            if (string.IsNullOrEmpty(cartCookie))
            {
                return new List<CartItem>();
            }
            return JsonConvert.DeserializeObject<List<CartItem>>(cartCookie) ?? new List<CartItem>();
        }




        // Details of Product
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //Get [About As]
        [HttpGet]
        public IActionResult AboutAs()
        {
            return View();
        }


    }
}
