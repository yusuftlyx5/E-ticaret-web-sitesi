using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EticaretApp.Models;

// Sipariş modeli
public class Order
{
    [Key]
    public int Id { get; set; }

    // UserId controller'da set edilir, form'dan gönderilmez
    [ScaffoldColumn(false)]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres zorunludur")]
    [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
    [Display(Name = "Teslimat Adresi")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur")]
    [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir")]
    [Display(Name = "Telefon")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Not en fazla 200 karakter olabilir")]
    [Display(Name = "Sipariş Notu")]
    public string? Note { get; set; }

    // TotalAmount controller'da hesaplanır, form'dan gönderilmez
    [ScaffoldColumn(false)]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Toplam Tutar")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "Sipariş Durumu")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Navigation property - Sipariş öğeleri
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [Display(Name = "Sipariş Tarihi")]
    public DateTime OrderDate { get; set; } = DateTime.Now;
}

// Sipariş durumu enum
public enum OrderStatus
{
    Pending = 0,      // Beklemede
    Processing = 1,   // İşleniyor
    Shipped = 2,      // Kargoya verildi
    Delivered = 3,    // Teslim edildi
    Cancelled = 4     // İptal edildi
}

