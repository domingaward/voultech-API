using System.Text.Json.Serialization;

namespace PurchaseOrderAPI.DTOs
{
    /// <summary>
    /// Estructura estándar para todas las respuestas de la API
    /// </summary>
    /// <typeparam name="T">Tipo de datos que se devuelve</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indica si la operación fue exitosa
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Datos de la respuesta (solo presente en operaciones exitosas)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        /// <summary>
        /// Detalles del error (solo presente cuando Success = false)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Error { get; set; }

        /// <summary>
        /// Timestamp de la respuesta
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Constructores para facilitar la creación de respuestas
        public static ApiResponse<T> SuccessResult(T data, string message = "Operación exitosa")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, object? error = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
    }

    /// <summary>
    /// Respuesta API sin datos específicos (para operaciones como DELETE)
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResult(string message = "Operación exitosa")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static new ApiResponse ErrorResult(string message, object? error = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
    }
}