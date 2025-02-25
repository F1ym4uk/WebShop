using System.ComponentModel.DataAnnotations;

namespace webshop.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Поле `Электронная почта` является обязательным")]
        [EmailAddress(ErrorMessage = "Некорректный email.")]
        public string Email { get; set; }
    }

}
