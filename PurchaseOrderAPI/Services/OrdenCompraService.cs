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

                // Calculate and update total
                var ordenCompleta = await _context.OrdenesCompra
                    .Include(o => o.OrdenProductos)
                    .FirstAsync(o => o.Id == orden.Id);

                ordenCompleta.CalcularTotal();
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
                    
                    // Reload the order with updated products to calculate total
                    var ordenCompleta = await _context.OrdenesCompra
                        .Include(o => o.OrdenProductos)
                        .ThenInclude(op => op.Producto)
                        .FirstAsync(o => o.Id == id);
                    
                    ordenCompleta.CalcularTotal();
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
    }
}