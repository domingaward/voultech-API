using Microsoft.EntityFrameworkCore;
using PurchaseOrderAPI.Data;
using PurchaseOrderAPI.DTOs;
using PurchaseOrderAPI.Models;

namespace PurchaseOrderAPI.Services
{
    public interface IOrdenCompraService
    {
        Task<IEnumerable<OrdenCompraDto>> GetAllAsync();
        Task<OrdenCompraDto?> GetByIdAsync(int id);
        Task<OrdenCompraDto> CreateAsync(CreateOrdenCompraDto createDto);
        Task<OrdenCompraDto?> UpdateAsync(int id, UpdateOrdenCompraDto updateDto);
        Task<bool> DeleteAsync(int id);
    }

    public class OrdenCompraService : IOrdenCompraService
    {
        private readonly ApplicationDbContext _context;

        public OrdenCompraService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrdenCompraDto>> GetAllAsync()
        {
            var ordenes = await _context.OrdenesCompra
                .Include(o => o.OrdenProductos)
                .ThenInclude(op => op.Producto)
                .ToListAsync();

            return ordenes.Select(MapToDto);
        }

        public async Task<OrdenCompraDto?> GetByIdAsync(int id)
        {
            var orden = await _context.OrdenesCompra
                .Include(o => o.OrdenProductos)
                .ThenInclude(op => op.Producto)
                .FirstOrDefaultAsync(o => o.Id == id);

            return orden != null ? MapToDto(orden) : null;
        }

        public async Task<OrdenCompraDto> CreateAsync(CreateOrdenCompraDto createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orden = new OrdenCompra
                {
                    Cliente = createDto.Cliente,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.OrdenesCompra.Add(orden);
                await _context.SaveChangesAsync();

                foreach (var item in createDto.OrdenProductos)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto == null)
                    {
                        throw new ArgumentException($"Producto con ID {item.ProductoId} no encontrado");
                    }

                    var ordenProducto = new OrdenProducto
                    {
                        OrdenId = orden.Id,
                        ProductoId = item.ProductoId
                    };

                    _context.OrdenProductos.Add(ordenProducto);
                }

                await _context.SaveChangesAsync();

                // Calculate total with dynamic discounts
                var ordenCompleta = await _context.OrdenesCompra
                    .Include(o => o.OrdenProductos)
                    .ThenInclude(op => op.Producto)
                    .FirstAsync(o => o.Id == orden.Id);

                CalcularTotalConDescuentos(ordenCompleta);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return await GetByIdAsync(orden.Id) ?? throw new InvalidOperationException("Error al crear la orden");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrdenCompraDto?> UpdateAsync(int id, UpdateOrdenCompraDto updateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orden = await _context.OrdenesCompra
                    .Include(o => o.OrdenProductos)
                    .FirstOrDefaultAsync(o => o.Id == id);
                
                if (orden == null) return null;

                // Update basic properties
                orden.Cliente = updateDto.Cliente;

                // Update products if provided
                if (updateDto.OrdenProductos != null)
                {
                    // Validate that the products list is not empty
                    if (updateDto.OrdenProductos.Count == 0)
                    {
                        throw new ArgumentException("Debe incluir al menos un producto en la orden");
                    }

                    // Validate that all new products exist
                    var newProductIds = updateDto.OrdenProductos.Select(p => p.ProductoId).ToList();
                    foreach (var productId in newProductIds)
                    {
                        var producto = await _context.Productos.FindAsync(productId);
                        if (producto == null)
                        {
                            throw new ArgumentException($"Producto con ID {productId} no encontrado");
                        }
                    }

                    // Get current product IDs in the order
                    var currentProductIds = orden.OrdenProductos.Select(op => op.ProductoId).ToList();

                    // STEP 1: Delete products that are no longer in the new list
                    var productsToDelete = orden.OrdenProductos
                        .Where(op => !newProductIds.Contains(op.ProductoId))
                        .ToList();

                    foreach (var productToDelete in productsToDelete)
                    {
                        _context.OrdenProductos.Remove(productToDelete);
                    }

                    // STEP 2: Keep products that remain in both lists (no action needed)
                    // These products already exist and will remain unchanged

                    // STEP 3: Add new products that weren't in the original order
                    var productsToAdd = newProductIds
                        .Where(newId => !currentProductIds.Contains(newId))
                        .ToList();

                    foreach (var productIdToAdd in productsToAdd)
                    {
                        var nuevoOrdenProducto = new OrdenProducto
                        {
                            OrdenId = orden.Id,
                            ProductoId = productIdToAdd
                        };

                        _context.OrdenProductos.Add(nuevoOrdenProducto);
                    }

                    // Save changes to update the OrdenProductos
                    await _context.SaveChangesAsync();
                    
                    // Reload the order with updated products to calculate total with discounts
                    var ordenCompleta = await _context.OrdenesCompra
                        .Include(o => o.OrdenProductos)
                        .ThenInclude(op => op.Producto)
                        .FirstAsync(o => o.Id == id);
                    
                    CalcularTotalConDescuentos(ordenCompleta);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetByIdAsync(id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var orden = await _context.OrdenesCompra
                .Include(o => o.OrdenProductos)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null) return false;

            _context.OrdenesCompra.Remove(orden);
            await _context.SaveChangesAsync();
            return true;
        }

        private static OrdenCompraDto MapToDto(OrdenCompra orden)
        {
            return new OrdenCompraDto
            {
                Id = orden.Id,
                Cliente = orden.Cliente,
                FechaCreacion = orden.FechaCreacion,
                Total = orden.Total,
                OrdenProductos = orden.OrdenProductos.Select(op => new OrdenProductoDto
                {
                    Id = op.Id,
                    ProductoId = op.ProductoId,
                    ProductoNombre = op.Producto?.Nombre ?? ""
                }).ToList()
            };
        }

        /// <summary>
        /// Calculate the order total applying dynamic discounts based on business rules
        /// </summary>
        /// <param name="orden">La orden de compra con productos cargados</param>
        private void CalcularTotalConDescuentos(OrdenCompra orden)
        {
            if (orden.OrdenProductos == null || !orden.OrdenProductos.Any())
            {
                orden.Total = 0;
                return;
            }

            // Subtotal without discounts
            decimal subtotal = orden.OrdenProductos.Sum(op => op.Producto?.Precio ?? 0);

            // Apply discounts based on business rules
            decimal totalConDescuentos = AplicarDescuentos(subtotal, orden.OrdenProductos.Count);

            orden.Total = totalConDescuentos;
        }

        /// <summary>
        /// Apply discounts based on defined business rules:
        /// - If total > $500, apply 10% discount
        /// - If more than 5 distinct products, apply additional 5% discount
        /// - Both discounts can stack up to a maximum of 15%
        /// </summary>
        /// <param name="subtotal">Subtotal antes de descuentos</param>
        /// <param name="cantidadProductos">Cantidad de productos distintos en la orden</param>
        /// <returns>Total con descuentos aplicados</returns>
        private static decimal AplicarDescuentos(decimal subtotal, int cantidadProductos)
        {
            decimal totalConDescuentos = subtotal;
            decimal porcentajeDescuentoTotal = 0;

            // Total > $500 => 10% discount
            if (subtotal > 500)
            {
                porcentajeDescuentoTotal += 0.10m;
            }

            // Total > 5 products => 5% additional discount
            if (cantidadProductos > 5)
            {
                porcentajeDescuentoTotal += 0.05m; 
            }

            // Apply total discount (max 15% if both conditions met)
            if (porcentajeDescuentoTotal > 0)
            {
                decimal montoDescuento = subtotal * porcentajeDescuentoTotal;
                totalConDescuentos = subtotal - montoDescuento;
            }

            // Round to 2 decimals to maintain monetary precision
            return Math.Round(totalConDescuentos, 2);
        }
    }
}