using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EticaretApp.Models;

// Sipariş öğesi modeli
public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Sipariş ID zorunludur")]
    [Display(Name = "Sipariş ID")]
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Ürün ID zorunludur")]
    [Display(Name = "Ürün ID")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır")]
    [Display(Name = "Miktar")]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Birim Fiyat")]
    public decimal UnitPrice { get; set; }

    [Display(Name = "Beden")]
    public string? Size { get; set; }

    // Navigation properties
    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    // Ara toplam hesapla
    [NotMapped]
    public decimal SubTotal
    {
        get
        {
            return Quantity * UnitPrice;
        }
    }
}

