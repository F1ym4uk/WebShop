using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace webshop.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле `Имя` является обязательным")]
        [StringLength(100)]
        [Display(Name = "Имя")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Поле `Электронная почта` является обязательным")]
        [StringLength(100)]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле `Пароль` является обязательным")]
        [StringLength(100)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Изображение")]
        public string Image { get; set; }

        [Required(ErrorMessage = "Поле `Админ` является обязательным")]
        [DefaultValue(false)]
        [Display(Name = "Админ(Да/Нет)")]
        public bool Isadmin { get; set; }
         
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }
        
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpires { get; set; }
        
        [DefaultValue(false)]
        [Display(Name = "Почта подтверждена")]
        public bool? IsEmailConfirmed { get; set; }
    }

}
