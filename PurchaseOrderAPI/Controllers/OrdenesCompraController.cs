using Microsoft.AspNetCore.Mvc;
using PurchaseOrderAPI.DTOs;
using PurchaseOrderAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace PurchaseOrderAPI.Controllers
{
    [ApiController]
    [Route("api/ordenes")]
    [Produces("application/json")]
    public class OrdenesCompraController : ControllerBase
    {
        private readonly IOrdenCompraService _ordenCompraService;
        private readonly ILogger<OrdenesCompraController> _logger;

        public OrdenesCompraController(IOrdenCompraService ordenCompraService, ILogger<OrdenesCompraController> logger)
        {
            _ordenCompraService = ordenCompraService;
            _logger = logger;
        }

        /// <summary>
        /// List all purchase orders
        /// </summary>
        /// <returns>Lista de órdenes de compra</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrdenCompraDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<OrdenCompraDto>>> GetOrdenesCompra()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las órdenes de compra");
                var ordenes = await _ordenCompraService.GetAllAsync();
                return Ok(new { success = true, data = ordenes, message = "Órdenes obtenidas exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las órdenes de compra");
                return StatusCode(500, new { success = false, message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific purchase order by ID
        /// </summary>
        /// <param name="id">ID de la orden de compra</param>
        /// <returns>Orden de compra específica</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<OrdenCompraDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<OrdenCompraDto>>> GetOrdenCompra([Range(1, int.MaxValue, ErrorMessage = "El ID debe ser mayor a 0")] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse.ErrorResult("Datos de entrada inválidos", ModelState));
                }

                _logger.LogInformation("Obteniendo orden de compra con ID: {Id}", id);
                var orden = await _ordenCompraService.GetByIdAsync(id);
                
                if (orden == null)
                {
                    _logger.LogWarning("Orden de compra con ID {Id} no encontrada", id);
                    return NotFound(ApiResponse.ErrorResult($"Orden de compra con ID {id} no encontrada"));
                }

                return Ok(ApiResponse<OrdenCompraDto>.SuccessResult(orden, "Orden obtenida exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la orden de compra con ID: {Id}", id);
                return StatusCode(500, ApiResponse.ErrorResult("Error interno del servidor", new { details = ex.Message }));
            }
        }

        /// <summary>
        /// Create a new purchase order with products
        /// </summary>
        /// <param name="createDto">Datos de la nueva orden de compra</param>
        /// <returns>Orden de compra creada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OrdenCompraDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<OrdenCompraDto>>> CreateOrdenCompra([FromBody] CreateOrdenCompraDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Datos de entrada inválidos para crear orden de compra: {@Errors}", ModelState);
                    return BadRequest(ApiResponse.ErrorResult("Datos de entrada inválidos", ModelState));
                }

                _logger.LogInformation("Creando nueva orden de compra para cliente: {Cliente}", createDto.Cliente);
                var orden = await _ordenCompraService.CreateAsync(createDto);

                return CreatedAtAction(
                    nameof(GetOrdenCompra), 
                    new { id = orden.Id }, 
                    ApiResponse<OrdenCompraDto>.SuccessResult(orden, "Orden de compra creada exitosamente")
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear orden de compra");
                return BadRequest(ApiResponse.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la orden de compra");
                return StatusCode(500, ApiResponse.ErrorResult("Error interno del servidor", new { details = ex.Message }));
            }
        }

        /// <summary>
        /// Update an existing purchase order
        /// </summary>
        /// <param name="id">ID de la orden de compra</param>
        /// <param name="updateDto">Datos actualizados de la orden</param>
        /// <returns>Orden de compra actualizada</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<OrdenCompraDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<OrdenCompraDto>>> UpdateOrdenCompra(
            [Range(1, int.MaxValue, ErrorMessage = "El ID debe ser mayor a 0")] int id,
            [FromBody] UpdateOrdenCompraDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Datos de entrada inválidos para actualizar orden de compra ID {Id}: {@Errors}", id, ModelState);
                    return BadRequest(ApiResponse.ErrorResult("Datos de entrada inválidos", ModelState));
                }

                _logger.LogInformation("Actualizando orden de compra con ID: {Id}", id);
                var orden = await _ordenCompraService.UpdateAsync(id, updateDto);

                if (orden == null)
                {
                    _logger.LogWarning("Orden de compra con ID {Id} no encontrada para actualizar", id);
                    return NotFound(ApiResponse.ErrorResult($"Orden de compra con ID {id} no encontrada"));
                }

                return Ok(ApiResponse<OrdenCompraDto>.SuccessResult(orden, "Orden de compra actualizada exitosamente"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar orden de compra ID {Id}", id);
                return BadRequest(ApiResponse.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la orden de compra con ID: {Id}", id);
                return StatusCode(500, ApiResponse.ErrorResult("Error interno del servidor", new { details = ex.Message }));
            }
        }

        /// <summary>
        /// Delete a purchase order by ID and all its associations
        /// </summary>
        /// <param name="id">ID de la orden de compra</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse>> DeleteOrdenCompra([Range(1, int.MaxValue, ErrorMessage = "El ID debe ser mayor a 0")] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse.ErrorResult("Datos de entrada inválidos", ModelState));
                }

                _logger.LogInformation("Eliminando orden de compra con ID: {Id}", id);
                var deleted = await _ordenCompraService.DeleteAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Orden de compra con ID {Id} no encontrada para eliminar", id);
                    return NotFound(ApiResponse.ErrorResult($"Orden de compra con ID {id} no encontrada"));
                }

                return Ok(ApiResponse.SuccessResult("Orden de compra eliminada exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la orden de compra con ID: {Id}", id);
                return StatusCode(500, ApiResponse.ErrorResult("Error interno del servidor", new { details = ex.Message }));
            }
        }
    }
}