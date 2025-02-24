using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace webshop.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        
        public string Image { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool Isadmin { get; set; }
         
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }
        
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpires { get; set; }
        
        [DefaultValue(false)]
        public bool? IsEmailConfirmed { get; set; }
    }

}
