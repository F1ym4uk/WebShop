using Microsoft.AspNetCore.Mvc;

namespace webshop.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            return statusCode switch
            {
                403 => View("AccessDenied"),
                404 => View("NotFound"),
                _ => View("Error")
            };
        }
    }
}