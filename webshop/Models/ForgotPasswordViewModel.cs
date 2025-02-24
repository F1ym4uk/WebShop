using System.ComponentModel.DataAnnotations;

namespace webshop.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Введите email.")]
        [EmailAddress(ErrorMessage = "Некорректный email.")]
        public string Email { get; set; }
    }



}
