using Microsoft.AspNetCore.Mvc;
using PurchaseOrderAPI.DTOs;
using PurchaseOrderAPI.Services;

namespace PurchaseOrderAPI.Controllers
{
    [ApiController]
    [Route("api/productos")]
    [Produces("application/json")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(IProductoService productoService, ILogger<ProductosController> logger)
        {
            _productoService = productoService;
            _logger = logger;
        }

        /// <summary>
        /// List all products
        /// </summary>
        /// <returns>List of products</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductoDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductoDto>>>> GetProductos()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los productos");
                var productos = await _productoService.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<ProductoDto>>.SuccessResult(productos, "Productos obtenidos exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los productos");
                return StatusCode(500, ApiResponse<IEnumerable<ProductoDto>>.ErrorResult(
                    "Error interno del servidor", 
                    new { details = ex.Message }));
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="createDto">New product data</param>
        /// <returns>Created product</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductoDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<ProductoDto>>> CreateProducto([FromBody] CreateProductoDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Datos de entrada inválidos para crear producto: {@Errors}", ModelState);
                    return BadRequest(ApiResponse.ErrorResult("Datos de entrada inválidos", ModelState));
                }

                _logger.LogInformation("Creando nuevo producto: {Nombre}", createDto.Nombre);
                var producto = await _productoService.CreateAsync(createDto);

                return CreatedAtAction(
                    nameof(GetProductos), // Changed to point to GetProductos since GetProducto no longer exists
                    null, // No route values needed since GetProductos doesn't take parameters
                    ApiResponse<ProductoDto>.SuccessResult(producto, "Producto creado exitosamente")
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear producto");
                return BadRequest(ApiResponse.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el producto");
                return StatusCode(500, ApiResponse.ErrorResult("Error interno del servidor", new { details = ex.Message }));
            }
        }
    }
}