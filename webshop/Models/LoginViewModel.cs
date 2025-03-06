using System.ComponentModel.DataAnnotations;

namespace webshop.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Обязательное поле!")]
        [EmailAddress(ErrorMessage = "Некорректный email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Обязательное поле!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

}
