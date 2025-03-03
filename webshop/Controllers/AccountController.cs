using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using webshop.Models;
using webshop.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;


namespace webshop.Controllers
{
    public class AccountController : Controller
    {
        
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<AccountController> _logger;
        private readonly EmailService _emailService;
        private const string CartCookieName = "UserCart";

        public AccountController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, ILogger<AccountController> logger, EmailService emailService)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _emailService = emailService;
        }


        //Get [Register]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        //Post [Register]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email уже зарегистрирован.");
                return View(model);
            }

            if (_context.Users.Any(u => u.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Этот номер телефона уже используется.");
                return View(model);
            }

            string imageFileName = "default.png";

            if (model.Image != null && model.Image.Length > 0)
            {
                string extension = Path.GetExtension(model.Image.FileName);
                imageFileName = $"{Guid.NewGuid()}{extension}";
                string usersDir = Path.Combine(_hostEnvironment.WebRootPath, "images/users");

                Directory.CreateDirectory(usersDir);

                string filePath = Path.Combine(usersDir, imageFileName);

                try
                {
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await model.Image.CopyToAsync(stream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении изображения.");
                    ModelState.AddModelError("Image", "Не удалось загрузить изображение.");
                    return View(model);
                }
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Image = imageFileName,
                Isadmin = false,
                IsEmailConfirmed = false,
                EmailConfirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                EmailConfirmationTokenExpires = DateTime.UtcNow.AddHours(1)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var confirmEmailLink = Url.Action("ConfirmEmail", "Account", new { token = user.EmailConfirmationToken }, Request.Scheme);

            await _emailService.SendEmailAsync(user.Email, "Подтверждение почты", $"Пожалуйста, подтвердите ваш email, перейдя по следующей ссылке: <a href=\"{confirmEmailLink}\">Подтвердить</a>");

            ViewBag.Message = "Ссылка-подтверждение отправлена на вашу почту.";
            return View(new RegisterViewModel());
        }

        //Get [Confirm Email]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Неверный токен.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token && u.EmailConfirmationTokenExpires > DateTime.UtcNow);

            if (user == null)
            {
                return BadRequest("Неверный или просроченный токен.");
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpires = null;

            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Account");
        }


        // GET [Login]
        [HttpGet]
        public IActionResult Login() => View();

        // POST [Login]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль.");
                return View(model);
            }

            if (!user.IsEmailConfirmed.GetValueOrDefault(false))
            {
                ModelState.AddModelError("", "Пожалуйста, подтвердите свою почту, чтобы войти.");
                return View(model);
            }

            await SignInUser(user, model.RememberMe);
            return RedirectToAction("Index", "Home");
        }


        // Sign in
        private async Task SignInUser(User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim("IsAdmin", user.Isadmin.ToString()),
                new Claim("Image", user.Image ?? "Users/default.webp")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
        }

        //Post [Log out]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }



        // GET [Account/Profile]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            return user == null ? NotFound() : View(user);
        }



        // POST [Account/Profile]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(User model, IFormFile imageFile)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            user.Name = model.Name;
            user.PhoneNumber = model.PhoneNumber;

            if (imageFile != null && imageFile.Length > 0)
            {
                string extension = Path.GetExtension(imageFile.FileName);
                string newFileName = $"{Guid.NewGuid()}{extension}";
                string imagesPath = Path.Combine(_hostEnvironment.WebRootPath, "images/Users");
                string newFilePath = Path.Combine(imagesPath, newFileName);

                Directory.CreateDirectory(imagesPath);

                try
                {
                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(user.Image) && user.Image != "default.webp")
                    {
                        string oldFilePath = Path.Combine(imagesPath, user.Image);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    user.Image = newFileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ошибка при сохранении изображения.");
                    return View(user);
                }
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var userForReReg = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            await SignInUser(userForReReg, false);
            TempData["SuccessMessage"] = "Профиль успешно обновлён!";
            return RedirectToAction("Profile");
        }




        // Запрос на восстановление пароля
        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Пользователь не найден.");
                return View(model);
            }

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);

            string subject = "Сброс пароля";
            string message = $"<p>Для сброса пароля нажмите <a href='{resetLink}'>сюда</a>.</p><p>Если это были не вы — проигнорируйте это письмо.</p>";
            await _emailService.SendEmailAsync(user.Email, subject, message);

            ViewBag.Message = "Ссылка для сброса отправлена на вашу почту.";
            return View(new ForgotPasswordViewModel());
        }



        // Get [Page for enter new password]
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest("Неверный токен.");
            return View(new ResetPasswordViewModel { Token = token });
        }

        // Post [Enter new password]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == model.Token && u.PasswordResetTokenExpires > DateTime.UtcNow);

            if (user == null)
            {
                ModelState.AddModelError("", "Недействительный или просроченный токен.");
                return View(model);
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Пароль успешно сброшен!";

            await SignInUser(user, rememberMe: false);

            return RedirectToAction("Index", "Home");
        }




        // GET: [Admin Panel]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Admin(string category, string name, decimal? minPrice, decimal? maxPrice)
        {
            var isAdminClaim = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;

            if (isAdminClaim == null || !bool.TryParse(isAdminClaim, out var isAdmin) || !isAdmin)
            {
                return Forbid();
            }

            var categories = await _context.Products.Select(p => p.Category).Distinct().ToListAsync();
            var names = await _context.Products.Select(p => p.Name).Distinct().ToListAsync();

            var products = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(category)) products = products.Where(p => p.Category == category);
            if (!string.IsNullOrEmpty(name)) products = products.Where(p => p.Name == name);
            if (minPrice.HasValue) products = products.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) products = products.Where(p => p.Price <= maxPrice.Value);

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;
            ViewBag.Names = names;
            ViewBag.SelectedName = name;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(products.ToList());
        }

        //Get [Users] for admin panel
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AdminUsers(string email, bool isadmin)
        {

            var isAdminClaim = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;

            if (isAdminClaim == null || !bool.TryParse(isAdminClaim, out var isAdmin) || !isAdmin)
            {
                return Forbid();
            }

            var emails = await _context.Users
                .Select(p => p.Email)
                .Distinct()
                .ToListAsync();

            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(email))
            {
                users = users.Where(p => p.Email == email);
            }

            if (isadmin)
            {
                users = users.Where(p => p.Isadmin);
            }

            ViewBag.Emails = emails;
            ViewBag.Selectedemail = email;
            
            return View(users.ToList());
        }


        // Get [Edit User]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditUser(int id)
        {
            var isAdminClaim = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;

            if (isAdminClaim == null || !bool.TryParse(isAdminClaim, out var isAdmin) || !isAdmin)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {id} not found.");
                return NotFound();
            }
            return View(user);

        }

        // POST [Edit User]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, string isadmin, User user, IFormFile imageFile)
        {

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                _logger.LogWarning($"User with ID {id} not found during update.");
                return NotFound();
            }

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            if ((!string.IsNullOrEmpty(isadmin)) && (isadmin == "Да")) existingUser.Isadmin = true;
            if ((!string.IsNullOrEmpty(isadmin)) && (isadmin == "Нет")) existingUser.Isadmin = false;

            if (imageFile != null)
            {
                string extension = Path.GetExtension(imageFile.FileName);
                string newFileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(_hostEnvironment.WebRootPath, "images/Users", newFileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(existingUser.Image))
                    {
                        string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, "images/Users", existingUser.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                            _logger.LogInformation($"Old image {existingUser.Image} deleted.");
                        }
                    }

                    existingUser.Image = newFileName;
                    _logger.LogInformation($"Image updated: {newFileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving new image.");
                    ModelState.AddModelError("", "Ошибка при сохранении изображения.");
                    return View(user);
                }
            }

            try
            {
                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product {existingUser.Name} updated successfully.");
                return RedirectToAction("AdminUsers");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating product.");
                ModelState.AddModelError("", "Ошибка при сохранении изменений.");
            }

            return View(user);
        }

        // Del user
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            ViewBag.Message = "Пользователь удалён";
            return RedirectToAction("AdminUsers");
        }



        // Get Add Product
        [HttpGet]
        [Authorize]
        public IActionResult AddProduct()
        {

            var isAdminClaim = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;

            if (isAdminClaim == null || !bool.TryParse(isAdminClaim, out var isAdmin) || !isAdmin)
            {
                return Forbid();
            }

            return View();
        }

        // Post Add Product
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile imageFile)
        {
            _logger.LogInformation("AddProduct action called.");


            if (imageFile != null)
            {
                string extension = Path.GetExtension(imageFile.FileName);
                string newFileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(_hostEnvironment.WebRootPath, "images/products", newFileName);

                string productsDir = Path.Combine(_hostEnvironment.WebRootPath, "products");

                Directory.CreateDirectory(productsDir);

                product.Image = newFileName;

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    product.Image = newFileName;
                    _logger.LogInformation($"Image saved successfully to {filePath}. product.Image = {product.Image}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving image.");
                    ModelState.AddModelError("", "Ошибка при сохранении изображения.");
                    return View(product);
                }
            }
            _logger.LogInformation($"\n\n{product.Image}\n\n");

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product {product.Name} added successfully.");
                return RedirectToAction(nameof(Admin));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product.");
                ModelState.AddModelError("", "Ошибка при сохранении товара.");
            }

            
            return View(product);
        }
        
        // Del product
        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Admin));
        }

        // Get [Edit product]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProduct(int id)
        {

            var isAdminClaim = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;

            if (isAdminClaim == null || !bool.TryParse(isAdminClaim, out var isAdmin) || !isAdmin)
            {
                return Forbid();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {id} not found.");
                return NotFound();
            }
            return View(product);
        }

        // POST [Edit Product]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile imageFile)
        {
            if (id != product.Id)
            {
                _logger.LogError($"Product ID mismatch. Provided: {id}, Product: {product.Id}");
                return BadRequest();
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning($"Product with ID {id} not found during update.");
                return NotFound();
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;

            if (imageFile != null)
            {
                string extension = Path.GetExtension(imageFile.FileName);
                string newFileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(_hostEnvironment.WebRootPath, "images/Products", newFileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(existingProduct.Image))
                    {
                        string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, "images/Products", existingProduct.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                            _logger.LogInformation($"Old image {existingProduct.Image} deleted.");
                        }
                    }

                    existingProduct.Image = newFileName;
                    _logger.LogInformation($"Image updated: {newFileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving new image.");
                    ModelState.AddModelError("", "Ошибка при сохранении изображения.");
                    return View(product);
                }
            }

            try
            {
                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product {existingProduct.Name} updated successfully.");
                return RedirectToAction("Admin");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating product.");
                ModelState.AddModelError("", "Ошибка при сохранении изменений.");
            }

            return View(product);
        }
    

    }

}

