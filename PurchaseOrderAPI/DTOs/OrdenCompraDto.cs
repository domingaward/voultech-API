using System.ComponentModel.DataAnnotations;

namespace PurchaseOrderAPI.DTOs
{
    public class OrdenCompraDto
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public decimal Total { get; set; }
        public List<OrdenProductoDto> OrdenProductos { get; set; } = new List<OrdenProductoDto>();
    }

    public class CreateOrdenCompraDto
    {
        [Required(ErrorMessage = "El cliente es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre del cliente debe tener entre 2 y 100 caracteres")]
        public string Cliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe incluir al menos un producto")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        // This validation can change but for now we limit to 50 products per order for simplicity
        [MaxLength(50, ErrorMessage = "No se pueden incluir más de 50 productos por orden")]
        public List<CreateOrdenProductoDto> OrdenProductos { get; set; } = new List<CreateOrdenProductoDto>();
    }

    public class UpdateOrdenCompraDto
    {
        [Required(ErrorMessage = "El cliente es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre del cliente debe tener entre 2 y 100 caracteres")]
        public string Cliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe incluir al menos un producto")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        // This validation can change but for now we limit to 50 products per order for simplicity
        [MaxLength(50, ErrorMessage = "No se pueden incluir más de 50 productos por orden")]
        public List<CreateOrdenProductoDto>? OrdenProductos { get; set; }
    }
}