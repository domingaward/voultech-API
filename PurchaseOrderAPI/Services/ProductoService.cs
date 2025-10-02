using Microsoft.EntityFrameworkCore;
using PurchaseOrderAPI.Data;
using PurchaseOrderAPI.DTOs;
using PurchaseOrderAPI.Models;

namespace PurchaseOrderAPI.Services
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductoDto>> GetAllAsync();
        Task<ProductoDto?> GetByIdAsync(int id);
        Task<ProductoDto> CreateAsync(CreateProductoDto createDto);
    }

    public class ProductoService : IProductoService
    {
        private readonly ApplicationDbContext _context;

        public ProductoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductoDto>> GetAllAsync()
        {
            var productos = await _context.Productos.ToListAsync();
            return productos.Select(MapToDto);
        }

        public async Task<ProductoDto?> GetByIdAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            return producto != null ? MapToDto(producto) : null;
        }

        public async Task<ProductoDto> CreateAsync(CreateProductoDto createDto)
        {
            var producto = new Producto
            {
                Nombre = createDto.Nombre,
                Precio = createDto.Precio
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return MapToDto(producto);
        }

        private static ProductoDto MapToDto(Producto producto)
        {
            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio
            };
        }
    }
}