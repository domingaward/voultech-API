using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseOrderAPI.Models
{
    public class OrdenCompra
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El cliente es requerido")]
        [StringLength(100, ErrorMessage = "El nombre del cliente no puede exceder 100 caracteres")]
        public string Cliente { get; set; } = string.Empty;

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // Navigation property
        public virtual ICollection<OrdenProducto> OrdenProductos { get; set; } = new List<OrdenProducto>();

        // Method to calculate total based on associated products (backward compatibility)
        // Note: This method calculates without discounts for backward compatibility
        // The service layer handles discount calculations
        public void CalcularTotal()
        {
            Total = OrdenProductos?.Sum(op => op.Producto?.Precio ?? 0) ?? 0;
        }
    }
}