using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseOrderAPI.Models
{
    public class OrdenProducto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrdenId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        // Navigation properties
        [ForeignKey("OrdenId")]
        public virtual OrdenCompra OrdenCompra { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; } = null!;
    }
}