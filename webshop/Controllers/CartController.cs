using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webshop.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using webshop.Data;

namespace webshop.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartCookieName = "UserCart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET [Index]
        public IActionResult Index()
        {
            var cart = GetCartFromCookie();
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await GetProductById(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCartFromCookie();
            var existingItem = cart.FirstOrDefault(item => item.Product.Id == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem { Product = product, Quantity = quantity });
            }

            SaveCartToCookie(cart);
            return RedirectToAction("Index", "Home");
        }

        // POST: [Remove from Cart]
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCartFromCookie();
            var itemToRemove = cart.FirstOrDefault(item => item.Product.Id == id);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCartToCookie(cart);
            }

            return RedirectToAction("Index");
        }

        // POST [Decreasing Quantity]
        [HttpPost]
        public IActionResult DecreaseQuantity(int id)
        {
            var cart = GetCartFromCookie();
            var itemToDecrease = cart.FirstOrDefault(item => item.Product.Id == id);

            if (itemToDecrease != null)
            {
                if (itemToDecrease.Quantity > 1)
                {
                    itemToDecrease.Quantity -= 1;
                }
                else
                {
                    cart.Remove(itemToDecrease);
                }

                SaveCartToCookie(cart);
            }

            return RedirectToAction("Index");
        }

        // POST [Increasing Quantity]
        [HttpPost]
        public IActionResult IncreasingQuantity(int id)
        {
            var cart = GetCartFromCookie();
            var itemToDecrease = cart.FirstOrDefault(item => item.Product.Id == id);

            if (itemToDecrease != null)
            {
                itemToDecrease.Quantity += 1;

                SaveCartToCookie(cart);
            }

            return RedirectToAction("Index");
        }

        // POST [Clear]
        [HttpPost]
        public IActionResult Clear()
        {
            Response.Cookies.Delete(CartCookieName);
            return RedirectToAction("Index");
        }

        // Get cart from cookie
        private List<CartItem> GetCartFromCookie()
        {
            var cartCookie = Request.Cookies[CartCookieName];
            if (string.IsNullOrEmpty(cartCookie))
            {
                return new List<CartItem>();
            }
            return JsonConvert.DeserializeObject<List<CartItem>>(cartCookie) ?? new List<CartItem>();
        }

        // Save cart to cookie
        private void SaveCartToCookie(List<CartItem> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                IsEssential = true // !Important
            };

            Response.Cookies.Append(CartCookieName, cartJson, cookieOptions);
        }

        // Get data from db
        private async Task<Product> GetProductById(int productId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
        }
    }
}