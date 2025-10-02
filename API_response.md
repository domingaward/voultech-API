# Estandarización de Respuestas API - Documentación

## Formato Estándar de Respuestas

Todas las respuestas de la API ahora siguen un formato consistente en JSON:

### Estructura Base

```json
{
  "success": boolean,
  "message": "string",
  "data": object | array | null,
  "error": object | null,
  "timestamp": "2025-10-02T12:00:00Z"
}
```

### Ejemplos de Respuestas

#### 1. Respuesta Exitosa con Datos (GET)

```json
{
  "success": true,
  "message": "Órdenes obtenidas exitosamente",
  "data": [
    {
      "id": 1,
      "cliente": "Juan Pérez",
      "fechaCreacion": "2025-10-02T10:30:00Z",
      "total": 1700.00,
      "ordenProductos": [
        {
          "id": 1,
          "productoId": 1,
          "productoNombre": "Laptop HP"
        }
      ]
    }
  ],
  "timestamp": "2025-10-02T12:00:00Z"
}
```

#### 2. Respuesta Exitosa de Creación (POST - 201)

```json
{
  "success": true,
  "message": "Orden de compra creada exitosamente",
  "data": {
    "id": 2,
    "cliente": "María García",
    "fechaCreacion": "2025-10-02T12:00:00Z",
    "total": 500.00,
    "ordenProductos": [
      {
        "id": 2,
        "productoId": 2,
        "productoNombre": "Mouse Logitech"
      }
    ]
  },
  "timestamp": "2025-10-02T12:00:00Z"
}
```

#### 3. Respuesta Exitosa Sin Datos (DELETE - 200)

```json
{
  "success": true,
  "message": "Orden de compra eliminada exitosamente",
  "timestamp": "2025-10-02T12:00:00Z"
}
```

#### 4. Error de Validación (400)

```json
{
  "success": false,
  "message": "Datos de entrada inválidos",
  "error": {
    "Cliente": ["El cliente es requerido"],
    "OrdenProductos": ["Debe incluir al menos un producto"]
  },
  "timestamp": "2025-10-02T12:00:00Z"
}
```

#### 5. Recurso No Encontrado (404)

```json
{
  "success": false,
  "message": "Orden de compra con ID 999 no encontrada",
  "timestamp": "2025-10-02T12:00:00Z"
}
```

#### 6. Error Interno del Servidor (500)

```json
{
  "success": false,
  "message": "Error interno del servidor",
  "error": {
    "details": "Connection timeout to database"
  },
  "timestamp": "2025-10-02T12:00:00Z"
}
```

## Beneficios de la Estandarización

1. **Consistencia**: Todas las respuestas siguen el mismo patrón
2. **Facilidad de uso**: Los clientes pueden manejar respuestas de manera uniforme
3. **Información clara**: Cada respuesta incluye estado, mensaje y timestamp
4. **Mejor debugging**: Los errores incluyen detalles específicos
5. **Compatibilidad**: El formato es compatible con estándares REST

## Implementación Técnica

### Clase ApiResponse<T>
- `Success`: Indica si la operación fue exitosa
- `Message`: Mensaje descriptivo del resultado
- `Data`: Datos de respuesta (solo en operaciones exitosas)
- `Error`: Detalles del error (solo en errores)
- `Timestamp`: Marca de tiempo UTC

### Métodos Helper
- `ApiResponse<T>.SuccessResult(data, message)`: Para respuestas exitosas
- `ApiResponse.SuccessResult(message)`: Para respuestas sin datos
- `ApiResponse.ErrorResult(message, error)`: Para errores

## Códigos de Estado HTTP

- **200 OK**: Operación exitosa con datos
- **201 Created**: Recurso creado exitosamente
- **400 Bad Request**: Error de validación
- **404 Not Found**: Recurso no encontrado
- **500 Internal Server Error**: Error del servidor