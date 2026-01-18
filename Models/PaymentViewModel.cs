using System.ComponentModel.DataAnnotations;

namespace EticaretApp.Models;

public class PaymentViewModel
{
    // Order details to carry over
    public Order Order { get; set; } = new Order();

    [Required(ErrorMessage = "Kart üzerindeki isim zorunludur")]
    [Display(Name = "Kart Üzerindeki İsim")]
    public string CardHolderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kart numarası zorunludur")]
    // [CreditCard] validasyonunu kaldırdık, her numara kabul edilsin
    [Display(Name = "Kart Numarası")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Son kullanma tarihi zorunludur")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Geçersiz tarih formatı (AA/YY)")]
    [Display(Name = "Son Kullanma Tarihi (AA/YY)")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVV zorunludur")]
    [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Geçersiz CVV")]
    [Display(Name = "CVV")]
    public string CVV { get; set; } = string.Empty;
}
