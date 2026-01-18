using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EticaretApp.Models;

// Sepet öğesi modeli
public class CartItem
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Sepet ID zorunludur")]
    [Display(Name = "Sepet ID")]
    public int CartId { get; set; }

    [Required(ErrorMessage = "Ürün ID zorunludur")]
    [Display(Name = "Ürün ID")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır")]
    [Display(Name = "Miktar")]
    public int Quantity { get; set; }

    [Display(Name = "Beden")]
    public string? Size { get; set; }

    // Navigation properties
    [ForeignKey("CartId")]
    public Cart? Cart { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    // Ara toplam hesapla
    [NotMapped]
    public decimal SubTotal
    {
        get
        {
            return Quantity * (Product?.Price ?? 0);
        }
    }
}

