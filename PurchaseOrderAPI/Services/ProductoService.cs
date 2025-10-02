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
            // Additional business validations beyond data annotations
            var existingProduct = await _context.Productos
                .FirstOrDefaultAsync(p => p.Nombre.ToLower() == createDto.Nombre.ToLower());
            
            if (existingProduct != null)
            {
                throw new ArgumentException($"Ya existe un producto con el nombre '{createDto.Nombre}'");
            }

            // Validate price range (additional validation beyond DTO)
            if (createDto.Precio < 0.01m)
            {
                throw new ArgumentException("El precio debe ser al menos $0.01");
            }

            // Validate name format
            if (string.IsNullOrWhiteSpace(createDto.Nombre))
            {
                throw new ArgumentException("El nombre del producto no puede estar vacío");
            }

            if (createDto.Nombre.Trim().Length < 2)
            {
                throw new ArgumentException("El nombre del producto debe tener al menos 2 caracteres");
            }

            var producto = new Producto
            {
                Nombre = createDto.Nombre.Trim(),
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