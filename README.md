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

3. **Configurar SQL Server LocalDB**
   
   Verificar que SQL Server LocalDB esté ejecutándose:
   ```powershell
   # Verificar estado de LocalDB
   sqllocaldb info "MSSQLLocalDB"
   
   # Si está detenido, iniciarlo
   sqllocaldb start "MSSQLLocalDB"
   ```

4. **Instalar Entity Framework Tools (si no está instalado)**
   ```bash
   # Instalar EF Tools compatible con .NET 7
   dotnet tool install --global dotnet-ef --version 7.0.20
   
   # Verificar instalación
   dotnet ef --version
   ```

5. **Aplicar migraciones de base de datos**
   ```bash
   # Aplicar migraciones (crear base de datos y tablas)
   dotnet ef database update
   ```
   
   Este comando:
   - Crea la base de datos `PurchaseOrderDB` en LocalDB
   - Crea las tablas: `OrdenesCompra`, `Productos`, `OrdenProductos`
   - Inserta datos de prueba (6 productos y 2 órdenes)

6. **Ejecutar la aplicación**
   ```bash
   dotnet run
   ```

   La API estará disponible en (según configuración en `launchSettings.json`):
   - **HTTP**: `http://localhost:5064`
   - **HTTPS**: `https://localhost:7020`
   - **Swagger UI**: `http://localhost:5064` o `https://localhost:7020`
   
   > **Nota**: Si los puertos están ocupados, .NET asignará puertos alternativos automáticamente. Siempre verifica la salida del comando `dotnet run` para confirmar las URLs exactas.

### Configuración de Base de Datos

La aplicación utiliza **Entity Framework Core** con migraciones para manejar el esquema de base de datos:

- **Connection String**: Configurada en `appsettings.json` para LocalDB
- **Migraciones**: Localizadas en `PurchaseOrderAPI/Migrations/`
- **Modelo**: Code First basado en las entidades en `Models/`

### Configuración de Puertos

Los puertos de la API están definidos en `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5064"
    },
    "https": {
      "applicationUrl": "https://localhost:7020;http://localhost:5064"
    }
  }
}
```

**Para cambiar los puertos:**
1. Edita `Properties/launchSettings.json`
2. Modifica los valores de `applicationUrl`
3. Reinicia la aplicación con `dotnet run`

**Comportamiento automático:**
- Si un puerto está ocupado, .NET asignará uno disponible automáticamente
- Siempre verifica la salida de `dotnet run` para confirmar las URLs exactas

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

A continuación se muestran ejemplos prácticos para utilizar todos los endpoints disponibles.

> **📁 Colección de Postman**: Puedes importar la colección completa de tests desde el archivo [`docs/postman/Voultech API Tests.postman_collection.json`](docs/postman/Voultech%20API%20Tests.postman_collection.json) para probar todos los endpoints directamente en Postman.

> **⚠️ Importante**: Los ejemplos usan `http://localhost:5064`, pero verifica la URL exacta en la salida de `dotnet run` ya que el puerto puede variar si está ocupado.

### Obtener todos los Productos
```bash
GET http://localhost:5064/api/productos
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
        },
        {
            "id": 3,
            "nombre": "Teclado Mecánico",
            "precio": 1200.00
        },
        {
            "id": 4,
            "nombre": "Monitor 24 pulgadas",
            "precio": 4500.00
        },
        {
            "id": 5,
            "nombre": "Impresora Multifuncional",
            "precio": 8000.00
        },
        {
            "id": 6,
            "nombre": "Silla Ergonómica",
            "precio": 3500.00
        }
    ],
    "timestamp": "2025-10-03T03:07:56.4577255Z"
}
```

**Captura de pantalla en Postman:**

<div align="center">
  <img src="docs/images/GET Productos.png" alt="GET Productos en Postman" width="800">
  <br>
  <em>Ejemplo de respuesta del endpoint GET /api/productos en Postman</em>
</div>

### Crear un Producto
```bash
POST http://localhost:5064/api/productos
Content-Type: application/json

{
  "nombre": "Webcam HD 1080p",
  "precio": 2500.00
}
```

**Respuesta exitosa (201 Created):**
```json
{
    "success": true,
    "message": "Producto creado exitosamente",
    "data": {
        "id": 7,
        "nombre": "Webcam HD 1080p",
        "precio": 2500.00
    },
    "timestamp": "2025-10-03T03:12:48.1054592Z"
}
```

**Captura de pantalla en Postman:**

<div align="center">
  <img src="docs/images/POST Productos.png" alt="POST Productos en Postman" width="800">
  <br>
  <em>Ejemplo de creación de producto con POST /api/productos en Postman</em>
</div>

### Obtener todas las Órdenes de Compra
```bash
GET http://localhost:5064/api/ordenes
```

**Respuesta exitosa:**
```json
{
    "success": true,
    "data": [
        {
            "id": 1,
            "cliente": "TechSolutions S.A.",
            "fechaCreacion": "2025-10-01T10:30:00",
            "total": 16700.00,
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
                    "id": 3,
                    "productoId": 3,
                    "productoNombre": "Teclado Mecánico"
                }
            ]
        },
        {
            "id": 2,
            "cliente": "Oficinas Corporativas Voultech",
            "fechaCreacion": "2025-10-02T14:15:00",
            "total": 16000.00,
            "ordenProductos": [
                {
                    "id": 4,
                    "productoId": 4,
                    "productoNombre": "Monitor 24 pulgadas"
                },
                {
                    "id": 5,
                    "productoId": 5,
                    "productoNombre": "Impresora Multifuncional"
                },
                {
                    "id": 6,
                    "productoId": 6,
                    "productoNombre": "Silla Ergonómica"
                }
            ]
        }
    ],
    "message": "Órdenes obtenidas exitosamente"
}
```

**Captura de pantalla en Postman:**

<div align="center">
  <img src="docs/images/GET Ordenes.png" alt="GET Órdenes en Postman" width="800">
  <br>
  <em>Ejemplo de respuesta del endpoint GET /api/ordenes en Postman</em>
</div>

### Obtener una Orden de Compra Específica (por ID)
```bash
GET http://localhost:5064/api/ordenes/1
```

**Respuesta exitosa:**
```json
{
    "success": true,
    "message": "Orden obtenida exitosamente",
    "data": {
        "id": 1,
        "cliente": "TechSolutions S.A.",
        "fechaCreacion": "2025-10-01T10:30:00",
        "total": 16700.00,
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
                "id": 3,
                "productoId": 3,
                "productoNombre": "Teclado Mecánico"
            }
        ]
    },
    "timestamp": "2025-10-03T03:16:54.1128915Z"
}
```

**Captura de pantalla en Postman:**

<div align="center">
  <img src="docs/images/GET Orden id.png" alt="GET Orden por ID en Postman" width="800">
  <br>
  <em>Ejemplo de respuesta del endpoint GET /api/ordenes/{id} en Postman</em>
</div>

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
POST http://localhost:5064/api/ordenes
Content-Type: application/json

{
  "cliente": "Empresa Innovadora Ltda.",
  "ordenProductos": [
    {
      "productoId": 1
    },
    {
      "productoId": 4
    },
    {
      "productoId": 5
    }
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
        "cliente": "Empresa Innovadora Ltda.",
        "fechaCreacion": "2025-10-03T03:29:17.9991821Z",
        "total": 24750.00,
        "ordenProductos": [
            {
                "id": 9,
                "productoId": 1,
                "productoNombre": "Laptop HP Pavilion"
            },
            {
                "id": 10,
                "productoId": 4,
                "productoNombre": "Monitor 24 pulgadas"
            },
            {
                "id": 11,
                "productoId": 5,
                "productoNombre": "Impresora Multifuncional"
            }
        ]
    },
    "timestamp": "2025-10-03T03:29:18.2502663Z"
}
```

**Captura de pantalla en Postman:**

<div align="center">
  <img src="docs/images/POST Orden 3.png" alt="POST Orden en Postman" width="800">
  <br>
  <em>Ejemplo de creación de orden con POST /api/ordenes en Postman</em>
</div>

### Editar una Orden de Compra Existente (por ID)
```bash
PUT http://localhost:5064/api/ordenes/3
Content-Type: application/json

{
  "cliente": "Empresa Innovadora Ltda. - Sucursal Norte",
  "ordenProductos": [
    {
      "productoId": 1
    },
    {
      "productoId": 2
    },
    {
      "productoId": 6
    }
  ]
}
```

**Respuesta exitosa:**
```json
{
    "success": true,
    "message": "Orden de compra actualizada exitosamente",
    "data": {
        "id": 3,
        "cliente": "Empresa Innovadora Ltda. - Sucursal Norte",
        "fechaCreacion": "2025-10-03T03:29:17.9991821",
        "total": 17100.00,
        "ordenProductos": [
            {
                "id": 9,
                "productoId": 1,
                "productoNombre": "Laptop HP Pavilion"
            },
            {
                "id": 12,
                "productoId": 2,
                "productoNombre": "Mouse Logitech"
            },
            {
                "id": 13,
                "productoId": 6,
                "productoNombre": "Silla Ergonómica"
            }
        ]
    },
    "timestamp": "2025-10-03T03:32:04.2987495Z"
}
```

**Captura de pantalla en Postman:**

<div align="center">
  <img src="docs/images/PUT Orden 3.png" alt="PUT Orden en Postman" width="800">
  <br>
  <em>Ejemplo de actualización de orden con PUT /api/ordenes/{id} en Postman</em>
</div>

### Eliminar una Orden de Compra y todas sus Asociaciones
```bash
DELETE http://localhost:5064/api/ordenes/3
```

**Respuesta exitosa:**
```json
{
    "success": true,
    "message": "Orden de compra eliminada exitosamente",
    "timestamp": "2025-10-03T03:33:47.3839167Z"
}
```

**Captura de pantalla en Postman:**

<div align="center">
  <img src="docs/images/DEL Orden 3.png" alt="DELETE Orden en Postman" width="800">
  <br>
  <em>Ejemplo de eliminación de orden con DELETE /api/ordenes/{id} en Postman</em>
</div>

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

## � Colección de Postman

Para facilitar las pruebas de la API, se incluye una colección completa de Postman con todos los endpoints configurados.

### 📁 **Ubicación del archivo:**
```
docs/postman/Voultech API Tests.postman_collection.json
```

### 🚀 **Cómo importar la colección:**

1. **Abrir Postman**
2. **Click en "Import"** (botón en la esquina superior izquierda)
3. **Seleccionar "Upload Files"**
4. **Navegar y seleccionar** el archivo `Voultech API Tests.postman_collection.json`
5. **Click "Import"**

### ⚙️ **Configuración recomendada:**

Una vez importada la colección, configura las siguientes variables:

1. **Crear Environment** (opcional pero recomendado):
   - **Variable**: `baseUrl`
   - **Valor**: `http://localhost:5064`

2. **Usar la variable en requests**:
   - Los endpoints ya están configurados para usar `{{baseUrl}}` si tienes la variable configurada
   - Si no usas variables, verifica que la URL base sea correcta en cada request

### 📋 **Contenido de la colección:**

La colección incluye requests para todos los endpoints:

- ✅ **GET** `/api/productos` - Listar productos
- ✅ **POST** `/api/productos` - Crear producto
- ✅ **GET** `/api/ordenes` - Listar órdenes
- ✅ **GET** `/api/ordenes/{id}` - Obtener orden específica
- ✅ **POST** `/api/ordenes` - Crear orden
- ✅ **PUT** `/api/ordenes/{id}` - Actualizar orden
- ✅ **DELETE** `/api/ordenes/{id}` - Eliminar orden

### 🧪 **Tests automatizados incluidos:**

Cada request incluye tests automáticos que verifican:
- Códigos de estado HTTP correctos
- Estructura de respuesta válida
- Presencia de campos requeridos
- Tipos de datos esperados

## � Reglas de Negocio

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