using System.ComponentModel.DataAnnotations;

namespace PurchaseOrderAPI.DTOs
{
    public class OrdenProductoDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
    }

    public class CreateOrdenProductoDto
    {
        [Required(ErrorMessage = "El ID del producto es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido")]
        public int ProductoId { get; set; }
    }
}