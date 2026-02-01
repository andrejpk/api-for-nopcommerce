# Entity Attributes Support

This document describes how to use generic attributes with entities in the API.

## Overview

Generic attributes allow you to store custom key-value pairs on entities without modifying the database schema. This is useful for storing integration-specific data, custom metadata, or temporary values.

## Supported Entities

The following entities now support generic attributes via the `IAttributeDto` interface:

- ✅ **Shipment** - `ShipmentDto`
- ✅ **Order** - `OrderDto`
- ✅ **Product** - `ProductDto`
- ✅ **Category** - `CategoryDto`
- ✅ **Manufacturer** - `ManufacturerDto`
- ✅ **Customer** - `CustomerDto` (via `BaseCustomerDto`)

## API Usage

### Setting Attributes (CREATE/UPDATE)

Include an `attributes` object in your JSON payload with string key-value pairs:

```json
PUT /api/orders/38974/shipments/57
{
  "shipment": {
    "tracking_number": "123456",
    "attributes": {
      "EasyPost.Shipment.Id": "shp_xxx",
      "Carrier": "USPS",
      "Service": "Priority",
      "CustomField": "CustomValue"
    }
  }
}
```

```json
POST /api/products
{
  "product": {
    "name": "Test Product",
    "attributes": {
      "ExternalId": "ext-12345",
      "Supplier": "Acme Corp"
    }
  }
}
```

### Getting Attributes (GET)

Attributes are automatically included in GET responses:

```json
GET /api/orders/38974/shipments/57

Response:
{
  "shipments": [{
    "id": 57,
    "tracking_number": "123456",
    "attributes": {
      "EasyPost.Shipment.Id": "shp_xxx",
      "Carrier": "USPS",
      "Service": "Priority"
    },
    ...
  }]
}
```

## Database Storage

Attributes are stored in the `GenericAttribute` table:

| Column | Description | Example |
|--------|-------------|---------|
| `EntityId` | The entity's ID | `57` |
| `KeyGroup` | The entity type name | `"Shipment"` |
| `Key` | The attribute key | `"EasyPost.Shipment.Id"` |
| `Value` | The attribute value | `"shp_xxx"` |
| `StoreId` | Store context | `0` (default) |

### Example Query

```sql
-- Get all attributes for a shipment
SELECT * FROM GenericAttribute 
WHERE KeyGroup = 'Shipment' 
  AND EntityId = 57;

-- Get all attributes for an order
SELECT * FROM GenericAttribute 
WHERE KeyGroup = 'Order' 
  AND EntityId = 38974;
```

## Implementation for Developers

### For New DTOs

To add attribute support to a new DTO:

#### 1. Implement IAttributeDto Interface

```csharp
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Base;

public class MyEntityDto : BaseDto, IAttributeDto
{
    private Dictionary<string, string> _attributes;

    // ... other properties ...

    /// <summary>
    /// Gets or sets the entity generic attributes
    /// </summary>
    [JsonProperty("attributes")]
    [DoNotMap]  // Important: Prevents Delta merge conflicts
    public Dictionary<string, string> Attributes
    {
        get => _attributes ??= new Dictionary<string, string>();
        set => _attributes = value;
    }
}
```

#### 2. Update Controller (CREATE/UPDATE)

```csharp
using Nop.Plugin.Api.Services;

public class MyEntityController : BaseApiController
{
    private readonly IEntityAttributeService _entityAttributeService;

    public MyEntityController(
        // ... other services ...
        IEntityAttributeService entityAttributeService)
        : base(/* ... */)
    {
        _entityAttributeService = entityAttributeService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMyEntity(/* ... */)
    {
        // ... create entity ...

        // Save attributes
        var attributes = _entityAttributeService.ExtractAttributesFromJson(
            Request.Body, 
            "my_entity"  // JSON root key
        );
        await _entityAttributeService.SaveAttributesAsync(newEntity, attributes);

        // ... return response ...
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMyEntity(/* ... */)
    {
        // ... update entity ...

        // Save attributes
        var attributes = _entityAttributeService.ExtractAttributesFromJson(
            Request.Body, 
            "my_entity"
        );
        await _entityAttributeService.SaveAttributesAsync(entityToUpdate, attributes);

        // ... return response ...
    }
}
```

#### 3. Update DTOHelper (GET)

```csharp
public async Task<MyEntityDto> PrepareMyEntityDTOAsync(MyEntity entity)
{
    var dto = entity.ToDto();
    
    // ... prepare other properties ...

    // Load attributes
    dto.Attributes = await _entityAttributeService.GetAttributesAsync<MyEntity>(entity.Id);

    return dto;
}
```

## EntityAttributeService API

### GetAttributesAsync<TEntity>

Loads attributes from the database for an entity.

```csharp
Dictionary<string, string> attributes = await _entityAttributeService
    .GetAttributesAsync<Shipment>(shipmentId);
```

### SaveAttributesAsync<TEntity>

Saves attributes to the database for an entity.

```csharp
var attributes = new Dictionary<string, string>
{
    ["Key1"] = "Value1",
    ["Key2"] = "Value2"
};
await _entityAttributeService.SaveAttributesAsync(entity, attributes);
```

### ExtractAttributesFromJson

Extracts attributes from the raw JSON request body.

```csharp
var attributes = _entityAttributeService.ExtractAttributesFromJson(
    Request.Body, 
    "shipment"  // Root JSON key
);
```

## Best Practices

1. **Naming Convention**: Use descriptive, namespaced keys to avoid conflicts
   - ✅ Good: `"EasyPost.Shipment.Id"`, `"Integration.ExternalId"`
   - ❌ Bad: `"id"`, `"temp"`, `"data"`

2. **Value Format**: Store simple string values
   - ✅ Good: `"12345"`, `"active"`, `"2024-01-15"`
   - ⚠️ Caution: Complex JSON as string (harder to query)

3. **Performance**: Attributes are loaded lazily
   - Only included when specifically requested via API
   - Not loaded by default in entity operations

4. **Validation**: Attributes are optional
   - Missing `attributes` in JSON is valid (no-op)
   - Empty `attributes` object is valid (no-op)

## Migration Notes

### Existing Data

If you previously stored attributes incorrectly (e.g., to Customer entity), you'll need to migrate:

```sql
-- Example: Move shipment attributes from Customer to Shipment
UPDATE GenericAttribute
SET EntityId = 57,  -- actual shipment ID
    KeyGroup = 'Shipment'
WHERE KeyGroup = 'Customer'
  AND EntityId = 1
  AND [Key] LIKE 'EasyPost.Shipment%';
```

### Breaking Changes

None. Adding attributes is backward compatible:
- Existing API calls without `attributes` continue to work
- GET responses now include `attributes` (may be null or empty)

## Examples

### Complete Shipment Example

```bash
# Create shipment with attributes
POST /api/orders/38974/shipments
{
  "shipment": {
    "shipment_items": [
      { "order_item_id": 123, "quantity": 1 }
    ],
    "attributes": {
      "EasyPost.Shipment.Id": "shp_xxx",
      "Carrier": "USPS"
    }
  }
}

# Update shipment attributes
PUT /api/orders/38974/shipments/57
{
  "shipment": {
    "tracking_number": "9400100208303111595196",
    "attributes": {
      "EasyPost.Shipment.Id": "shp_updated",
      "Status": "in_transit"
    }
  }
}

# Get shipment with attributes
GET /api/orders/38974/shipments/57
```

### Complete Order Example

```bash
# Create order with attributes
POST /api/orders
{
  "order": {
    "customer_id": 1,
    "attributes": {
      "SalesChannel": "Amazon",
      "AmazonOrderId": "amz-123"
    }
  }
}

# Get order with attributes
GET /api/orders/38974
```

## Support

For questions or issues:
1. Check the database `GenericAttribute` table to verify storage
2. Enable API logging to see request/response payloads
3. Verify DTO implements `IAttributeDto` interface
4. Ensure controller uses `IEntityAttributeService`
