using System.ComponentModel.DataAnnotations;

namespace webshop.Models
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Обязательное поле!")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Обязательное поле!")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов.")]
        [Display(Name = "Пароль")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Обязательное поле!")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают.")]
        [Display(Name = "Повторите пароль")]
        public string ConfirmPassword { get; set; }
    }



}
