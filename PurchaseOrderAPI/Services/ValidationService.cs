using Microsoft.EntityFrameworkCore;
using PurchaseOrderAPI.Data;
using PurchaseOrderAPI.DTOs;

namespace PurchaseOrderAPI.Services
{
    public interface IValidationService
    {
        Task<ValidationResult> ValidateProductExistsAsync(int productId);
        Task<ValidationResult> ValidateProductsExistAsync(IEnumerable<int> productIds);
        ValidationResult ValidateProductPrice(decimal price);
        ValidationResult ValidateOrderProductDuplicates(IEnumerable<CreateOrdenProductoDto> productos);
        Task<ValidationResult> ValidateCreateOrdenAsync(CreateOrdenCompraDto createDto);
        Task<ValidationResult> ValidateUpdateOrdenAsync(UpdateOrdenCompraDto updateDto);
    }

    public class ValidationService : IValidationService
    {
        private readonly ApplicationDbContext _context;

        public ValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ValidationResult> ValidateProductExistsAsync(int productId)
        {
            var exists = await _context.Productos.AnyAsync(p => p.Id == productId);
            
            if (!exists)
            {
                return ValidationResult.Error($"El producto con ID {productId} no existe");
            }

            return ValidationResult.Success();
        }

        public async Task<ValidationResult> ValidateProductsExistAsync(IEnumerable<int> productIds)
        {
            var uniqueProductIds = productIds.Distinct().ToList();
            var existingProducts = await _context.Productos
                .Where(p => uniqueProductIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var missingProducts = uniqueProductIds.Except(existingProducts).ToList();

            if (missingProducts.Any())
            {
                return ValidationResult.Error(
                    $"Los siguientes productos no existen: {string.Join(", ", missingProducts)}"
                );
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateProductPrice(decimal price)
        {
            if (price <= 0)
            {
                return ValidationResult.Error("El precio debe ser mayor a 0");
            }

            if (price > 999999.99m)
            {
                return ValidationResult.Error("El precio no puede exceder $999,999.99");
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateOrderProductDuplicates(IEnumerable<CreateOrdenProductoDto> productos)
        {
            var productIds = productos.Select(p => p.ProductoId).ToList();
            var duplicates = productIds.GroupBy(id => id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
            {
                return ValidationResult.Error(
                    $"Los siguientes productos están duplicados en la orden: {string.Join(", ", duplicates)}"
                );
            }

            return ValidationResult.Success();
        }

        public async Task<ValidationResult> ValidateCreateOrdenAsync(CreateOrdenCompraDto createDto)
        {
            // 1. Validate no duplicate products
            var duplicateValidation = ValidateOrderProductDuplicates(createDto.OrdenProductos);
            if (!duplicateValidation.IsValid)
            {
                return duplicateValidation;
            }

            // 2. Validate all products exist
            var productIds = createDto.OrdenProductos.Select(p => p.ProductoId);
            var productExistenceValidation = await ValidateProductsExistAsync(productIds);
            if (!productExistenceValidation.IsValid)
            {
                return productExistenceValidation;
            }

            // 3. Validate business rules
            if (createDto.OrdenProductos.Count > 50)
            {
                return ValidationResult.Error("No se pueden incluir más de 50 productos por orden");
            }

            return ValidationResult.Success();
        }

        public async Task<ValidationResult> ValidateUpdateOrdenAsync(UpdateOrdenCompraDto updateDto)
        {
            if (updateDto.OrdenProductos == null)
            {
                return ValidationResult.Success(); // Products are not being updated
            }

            // 1. Validate no duplicate products
            var duplicateValidation = ValidateOrderProductDuplicates(updateDto.OrdenProductos);
            if (!duplicateValidation.IsValid)
            {
                return duplicateValidation;
            }

            // 2. Validate all products exist
            var productIds = updateDto.OrdenProductos.Select(p => p.ProductoId);
            var productExistenceValidation = await ValidateProductsExistAsync(productIds);
            if (!productExistenceValidation.IsValid)
            {
                return productExistenceValidation;
            }

            // 3. Validate business rules
            if (updateDto.OrdenProductos.Count > 50)
            {
                return ValidationResult.Error("No se pueden incluir más de 50 productos por orden");
            }

            return ValidationResult.Success();
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        private ValidationResult(bool isValid, string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Success() => new(true);
        public static ValidationResult Error(string message) => new(false, message);
    }
}