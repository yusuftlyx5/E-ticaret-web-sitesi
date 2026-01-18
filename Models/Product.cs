using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EticaretApp.Models;

// Ürün modeli
public class Product
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunludur")]
    [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir")]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Fiyat zorunludur")]
    [Range(0.01, 999999.99, ErrorMessage = "Fiyat 0.01 ile 999999.99 arasında olmalıdır")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Fiyat")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stok miktarı zorunludur")]
    [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
    [Display(Name = "Stok Miktarı")]
    public int Stock { get; set; }

    [Display(Name = "Beden")]
    public string? Size { get; set; }

    [Display(Name = "Resim URL")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Kategori seçimi zorunludur")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    // Navigation property
    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    // Navigation property - Yorumlar
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    [Display(Name = "Oluşturulma Tarihi")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}

