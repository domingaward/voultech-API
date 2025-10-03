# Voultech Purchase Order API

API REST para la gestión de órdenes de compra y productos desarrollada con .NET 7, Entity Framework Core y SQL Server LocalDB.

## 📋 Tabla de Contenidos

- [Características](#características)
- [Tecnologías](#tecnologías)
- [Modelos de Datos](#modelos-de-datos)
- [Endpoints de la API](#endpoints-de-la-api)
- [Estructura de Respuestas](#estructura-de-respuestas)
- [Validaciones Implementadas](#validaciones-implementadas)
- [Configuración e Instalación](#configuración-e-instalación)
- [Uso de la API](#uso-de-la-api)
- [Reglas de Negocio](#reglas-de-negocio)

## ✨ Características

- **CRUD completo** para productos y órdenes de compra
- **Cálculo dinámico de descuentos** basado en reglas de negocio
- **Validaciones robustas** en múltiples capas (DTO, Service, Controller)
- **Respuestas estandarizadas** con formato JSON consistente
- **Manejo global de errores** con mensajes descriptivos
- **Documentación automática** con Swagger/OpenAPI
- **Logging estructurado** para monitoreo y debugging
- **Arquitectura en capas** siguiendo principios SOLID

## 🛠️ Tecnologías

- **.NET 7.0** - Framework principal
- **ASP.NET Core Web API** - Framework web
- **Entity Framework Core 7.0** - ORM para acceso a datos
- **SQL Server LocalDB** - Base de datos para desarrollo
- **Swagger/OpenAPI** - Documentación de API
- **AutoMapper** (implícito) - Mapeo de entidades a DTOs

## 📊 Modelos de Datos

### Producto
```csharp
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; }     // Máximo 100 caracteres, único
    public decimal Precio { get; set; }    // Rango: $0.01 - $999,999.99
}
```

### Orden de Compra
```csharp
public class OrdenCompra
{
    public int Id { get; set; }
    public string Cliente { get; set; }           // 2-100 caracteres
    public DateTime FechaCreacion { get; set; }   // UTC automático
    public decimal Total { get; set; }            // Calculado con descuentos
    public List<OrdenProducto> OrdenProductos { get; set; }
}
```

### Relación Orden-Producto
```csharp
public class OrdenProducto
{
    public int Id { get; set; }
    public int OrdenId { get; set; }
    public int ProductoId { get; set; }
}
```

### Relaciones
- Una **OrdenCompra** puede tener múltiples **Productos** (muchos a muchos)
- Una **OrdenCompra** se elimina en cascada con sus **OrdenProductos**
- Un **Producto** no se puede eliminar si está referenciado en órdenes

## 🔗 Endpoints de la API

### Productos (`/api/productos`)

| Método | Endpoint | Descripción | Validaciones |
|--------|----------|-------------|-------------|
| `GET` | `/api/productos` | Obtener todos los productos | - |
| `POST` | `/api/productos` | Crear nuevo producto | Nombre único, precio válido |

#### Validaciones de Productos:
- **Nombre**: Requerido, entre 2-100 caracteres y único en el sistema
- **Precio**: Requerido y debe ser sobre $0.00
- **Formato**: Sin espacios vacíos, trimming automático

### Órdenes de Compra (`/api/ordenes`)

| Método | Endpoint | Descripción | Validaciones |
|--------|----------|-------------|-------------|
| `GET` | `/api/ordenes` | Obtener todas las órdenes | - |
| `GET` | `/api/ordenes/{id}` | Obtener orden específica | ID > 0 |
| `POST` | `/api/ordenes` | Crear nueva orden | Cliente, productos válidos |
| `PUT` | `/api/ordenes/{id}` | Actualizar orden existente | ID > 0, datos válidos |
| `DELETE` | `/api/ordenes/{id}` | Eliminar orden | ID > 0 |

#### Validaciones de Órdenes:
- **Cliente**: Requerido, entre 2-100 caracteres y sin espacios vacíos
- **Productos**: Mínimo 1, máximo 50 productos por orden (esta decisión se tomo por simplicidad de una orden de compra)
- **IDs de Productos**: Deben existir en el catálogo
- **Duplicados**: No se permiten productos duplicados en la misma orden

## 📨 Estructura de Respuestas

Todas las respuestas de la API siguen un formato estandarizado:

### Respuesta Exitosa
```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": { /* datos específicos */ },
  "timestamp": "2025-10-02T12:00:00Z"
}
```

### Respuesta de Error
```json
{
  "success": false,
  "message": "Descripción del error",
  "error": { /* detalles del error */ },
  "timestamp": "2025-10-02T12:00:00Z"
}
```

### Códigos de Estado HTTP
- **200 OK**: Operación exitosa
- **201 Created**: Recurso creado exitosamente
- **400 Bad Request**: Error de validación
- **404 Not Found**: Recurso no encontrado
- **500 Internal Server Error**: Error del servidor

## ✅ Validaciones Implementadas

### Nivel DTO (Data Transfer Objects)
- Validaciones de atributos con `DataAnnotations`
- Rangos de valores y longitudes de cadenas
- Campos requeridos y formatos específicos

### Nivel Servicio (Business Logic)
- **Existencia de productos** antes de crear/actualizar órdenes
- **Detección de duplicados** en productos y órdenes
- **Reglas de negocio** específicas del dominio
- **Validación de nombres únicos** en productos

### Nivel Controlador (API Layer)
- **Validación de parámetros** de entrada
- **Sanitización de datos** (trimming, espacios vacíos)
- **Validaciones adicionales** de formato y contenido

## ⚙️ Configuración e Instalación

### Prerequisitos
- .NET 7.0 SDK
- SQL Server LocalDB
- Git

### Instalación

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/domingaward/voultech-API.git
   cd voultech-API/PurchaseOrderAPI
   ```

2. **Restaurar dependencias**
   ```bash
   dotnet restore
   ```

3. **Configurar la base de datos**
   
   Verificar que SQL Server LocalDB esté ejecutándose:
   ```powershell
   sqllocaldb info "MSSQLLocalDB"
   sqllocaldb start "MSSQLLocalDB"  # Si está detenido
   ```

4. **Ejecutar la aplicación**
   ```bash
   dotnet run
   ```

   La API estará disponible en:
   - **HTTP**: `http://localhost:5000`
   - **HTTPS**: `https://localhost:5001`
   - **Swagger UI**: `https://localhost:5001` (raíz)

### Configuración de Base de Datos

La aplicación utiliza **Code First** con Entity Framework Core. La base de datos se crea automáticamente al ejecutar la aplicación por primera vez usando `context.Database.EnsureCreated()`.

#### Datos de Ejemplo (Seed Data)
La aplicación incluye datos de prueba:

**Productos:**
- Laptop HP Pavilion ($15,000.00)
- Mouse Logitech ($500.00)
- Teclado Mecánico ($1,200.00)
- Monitor 24 pulgadas ($4,500.00)
- Impresora Multifuncional ($8,000.00)
- Silla Ergonómica ($3,500.00)

**Órdenes de Compra:**
- TechSolutions S.A. (3 productos)
- Oficinas Corporativas Voultech (3 productos)

## 🚀 Uso de la API

A continuación se muestran ejemplos prácticos para utilizar todos los endpoints disponibles:

### Obtener todos los Productos
```bash
GET /api/productos
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "message": "Productos obtenidos exitosamente",
  "data": [
    {
      "id": 1,
      "nombre": "Laptop HP Pavilion",
      "precio": 15000.00
    },
    {
      "id": 2,
      "nombre": "Mouse Logitech",
      "precio": 500.00
    }
  ],
  "timestamp": "2025-10-02T12:00:00Z"
}
```

### Crear un Producto
```bash
POST /api/productos
Content-Type: application/json

{
  "nombre": "Nuevo Producto",
  "precio": 150.50
}
```

**Respuesta exitosa (201 Created):**
```json
{
  "success": true,
  "message": "Producto creado exitosamente",
  "data": {
    "id": 7,
    "nombre": "Nuevo Producto",
    "precio": 150.50
  },
  "timestamp": "2025-10-02T12:00:00Z"
}
```

### Obtener todas las Órdenes de Compra
```bash
GET /api/ordenes
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "cliente": "TechSolutions S.A.",
      "fechaCreacion": "2025-10-01T10:30:00Z",
      "total": 15030.00,
      "ordenProductos": [
        {
          "id": 1,
          "productoId": 1,
          "productoNombre": "Laptop HP Pavilion"
        },
        {
          "id": 2,
          "productoId": 2,
          "productoNombre": "Mouse Logitech"
        }
      ]
    }
  ],
  "message": "Órdenes obtenidas exitosamente",
  "timestamp": "2025-10-02T12:00:00Z"
}
```

### Obtener una Orden de Compra Específica (por ID)
```bash
GET /api/ordenes/1
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "message": "Orden obtenida exitosamente",
  "data": {
    "id": 1,
    "cliente": "TechSolutions S.A.",
    "fechaCreacion": "2025-10-01T10:30:00Z",
    "total": 15030.00,
    "ordenProductos": [
      {
        "id": 1,
        "productoId": 1,
        "productoNombre": "Laptop HP Pavilion"
      },
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

**Respuesta de error (404 Not Found):**
```json
{
  "success": false,
  "message": "Orden de compra con ID 999 no encontrada",
  "timestamp": "2025-10-02T12:00:00Z"
}
```

### Crear una Orden de Compra
```bash
POST /api/ordenes
Content-Type: application/json

{
  "cliente": "Cliente Ejemplo S.A.",
  "ordenProductos": [
    { "productoId": 1 },
    { "productoId": 3 },
    { "productoId": 4 }
  ]
}
```

**Respuesta exitosa (201 Created):**
```json
{
  "success": true,
  "message": "Orden de compra creada exitosamente",
  "data": {
    "id": 3,
    "cliente": "Cliente Ejemplo S.A.",
    "fechaCreacion": "2025-10-02T12:00:00Z",
    "total": 18630.00,
    "ordenProductos": [
      {
        "id": 7,
        "productoId": 1,
        "productoNombre": "Laptop HP Pavilion"
      },
      {
        "id": 8,
        "productoId": 3,
        "productoNombre": "Teclado Mecánico"
      },
      {
        "id": 9,
        "productoId": 4,
        "productoNombre": "Monitor 24 pulgadas"
      }
    ]
  },
  "timestamp": "2025-10-02T12:00:00Z"
}
```

### Editar una Orden de Compra Existente (por ID)
```bash
PUT /api/ordenes/1
Content-Type: application/json

{
  "cliente": "TechSolutions S.A. - Actualizado",
  "ordenProductos": [
    { "productoId": 1 },
    { "productoId": 2 },
    { "productoId": 5 }
  ]
}
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "message": "Orden de compra actualizada exitosamente",
  "data": {
    "id": 1,
    "cliente": "TechSolutions S.A. - Actualizado",
    "fechaCreacion": "2025-10-01T10:30:00Z",
    "total": 21150.00,
    "ordenProductos": [
      {
        "id": 1,
        "productoId": 1,
        "productoNombre": "Laptop HP Pavilion"
      },
      {
        "id": 2,
        "productoId": 2,
        "productoNombre": "Mouse Logitech"
      },
      {
        "id": 10,
        "productoId": 5,
        "productoNombre": "Impresora Multifuncional"
      }
    ]
  },
  "timestamp": "2025-10-02T12:00:00Z"
}
```

### Eliminar una Orden de Compra y todas sus Asociaciones
```bash
DELETE /api/ordenes/1
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "message": "Orden de compra eliminada exitosamente",
  "timestamp": "2025-10-02T12:00:00Z"
}
```

**Respuesta de error (404 Not Found):**
```json
{
  "success": false,
  "message": "Orden de compra con ID 999 no encontrada",
  "timestamp": "2025-10-02T12:00:00Z"
}
```

### Ejemplos de Errores de Validación

**Error al crear producto con nombre duplicado:**
```json
{
  "success": false,
  "message": "Ya existe un producto con el nombre 'Laptop HP Pavilion'",
  "timestamp": "2025-10-02T12:00:00Z"
}
```

**Error al crear orden con productos inexistentes:**
```json
{
  "success": false,
  "message": "Los siguientes productos no existen: 999, 1000",
  "timestamp": "2025-10-02T12:00:00Z"
}
```

**Error de validación en datos de entrada:**
```json
{
  "success": false,
  "message": "Datos de entrada inválidos",
  "error": {
    "Cliente": ["El nombre del cliente debe tener entre 2 y 100 caracteres"],
    "OrdenProductos": ["Debe incluir al menos un producto"]
  },
  "timestamp": "2025-10-02T12:00:00Z"
}
```

## 💼 Reglas de Negocio

### Cálculo de Descuentos Dinámicos
El sistema aplica descuentos automáticos basados en las siguientes reglas:

1. **Descuento por Monto**: 10% si el subtotal > $500
2. **Descuento por Cantidad**: 5% adicional si > 5 productos distintos
3. **Acumulación**: Los descuentos se pueden acumular hasta un máximo del 15%

#### Ejemplo de Cálculo:
```
Subtotal: $16,700 (Laptop + Mouse + Teclado)
- Descuento por monto (>$500): 10%
- Productos: 3 (no aplica descuento por cantidad)
Total con descuento: $15,030.00
```

### Limitaciones de Negocio
- Máximo 50 productos por orden
- No se permiten productos duplicados en la misma orden
- Los nombres de productos deben ser únicos
- Los precios deben ser sobre $0.00 

## 🔧 Consideraciones Técnicas

### Logging
La aplicación incluye logging estructurado que registra:
- Operaciones CRUD exitosas
- Errores de validación con detalles
- Excepciones del sistema
- Información de rendimiento

### Manejo de Errores
- **Validaciones**: Mensajes específicos y útiles
- **Excepciones**: Logging detallado para debugging
- **Respuestas**: Formato consistente con códigos HTTP apropiados

### Transacciones
Las operaciones complejas (crear/actualizar órdenes) utilizan transacciones de base de datos para garantizar la consistencia de los datos.