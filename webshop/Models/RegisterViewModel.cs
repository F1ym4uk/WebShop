using System.ComponentModel.DataAnnotations;

namespace webshop.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Обязательное поле!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Обязательное поле!")]
        [EmailAddress]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Введите корректный номер телефона")]
        [RegularExpression(@"^\+375\((29|33|44|25)\)\d{3}-\d{2}-\d{2}$", ErrorMessage = "Формат номера: +375(XX)XXX-XX-XX")]
        [Required(ErrorMessage = "Обязательное поле!")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Обязательное поле!")]
        public string Password { get; set; }

        public IFormFile Image { get; set; }
    }
}
