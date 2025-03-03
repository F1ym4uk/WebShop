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

        [Required(ErrorMessage = "Поле `Номер телефона` является обязательным")]
        [Phone(ErrorMessage = "Введите корректный номер телефона")]
        [RegularExpression(@"^\+375\((29|33|44|25)\)\d{3}-\d{2}-\d{2}$", ErrorMessage = "Формат номера: +375(XX)XXX-XX-XX, где XX — 29, 33, 44 или 25")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Поле `Пароль` является обязательным")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public IFormFile Image { get; set; }
    }
}
