using System.ComponentModel.DataAnnotations;

namespace EticaretApp.Models;

// Kategori modeli
public class Category
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur")]
    [StringLength(50, ErrorMessage = "Kategori adı en fazla 50 karakter olabilir")]
    [Display(Name = "Kategori Adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    // Navigation property - Ürünler
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

