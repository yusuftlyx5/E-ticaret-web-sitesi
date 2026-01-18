using System.ComponentModel.DataAnnotations;

namespace EticaretApp.Models;

// Register ViewModel
public class RegisterViewModel
{
    [Required(ErrorMessage = "E-posta adresi zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur")]
    [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

















































