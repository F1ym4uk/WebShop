using System.ComponentModel.DataAnnotations;

namespace webshop.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Поле `Имя` является обязательным")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Поле `Электронная почта` является обязательным")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле `Пароль` является обязательным")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public IFormFile Image { get; set; }
    }

}
