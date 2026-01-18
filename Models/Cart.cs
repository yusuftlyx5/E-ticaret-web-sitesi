using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EticaretApp.Models;

// Sepet modeli
public class Cart
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Kullanıcı ID")]
    public string UserId { get; set; } = string.Empty;

    // Navigation property - Sepet öğeleri
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [Display(Name = "Oluşturulma Tarihi")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Güncellenme Tarihi")]
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    // Toplam tutarı hesapla
    [NotMapped]
    public decimal TotalAmount
    {
        get
        {
            return CartItems?.Sum(ci => ci.Quantity * ci.Product?.Price ?? 0) ?? 0;
        }
    }
}

