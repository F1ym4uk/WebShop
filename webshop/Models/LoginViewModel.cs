using System.ComponentModel.DataAnnotations;

namespace webshop.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле `Электронная почта` является обязательным")]
        [EmailAddress(ErrorMessage = "Некорректный email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле `Пароль` является обязательным")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

}
