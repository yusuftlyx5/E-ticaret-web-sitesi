using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EticaretApp.Models;

// Yorum modeli
public class Review
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Kullanıcı ID")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ürün ID zorunludur")]
    [Display(Name = "Ürün ID")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Yorum zorunludur")]
    [StringLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir")]
    [Display(Name = "Yorum")]
    public string Comment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Puan zorunludur")]
    [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır")]
    [Display(Name = "Puan")]
    public int Rating { get; set; }

    // Navigation property
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [Display(Name = "Yorum Tarihi")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}

