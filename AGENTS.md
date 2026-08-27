# Pnara - Restaurant POS & Order Management System

## Architecture

Clean Architecture with four layers:

- **Domain** (`Domain/`): Entities, Value Objects, Enums, Domain Events, Repository Interfaces, Domain Exceptions
- **Application** (`Application/`): Services, DTOs, Mappers, Interfaces, Use Cases
- **Infrastructure** (`Infrastructure/`): EF Core DbContext, Repository Implementations, Auth, Seeding, External Adapters
- **WebApi** (`WebApi/`): Controllers, Middleware, Authorization Policies, Program.cs

Stack: .NET 10, EF Core, PostgreSQL (Npgsql), JWT Auth

## Key Domain Entities

### Order Lifecycle
```
Draft → Registered → Invoiced → Paid
Draft → Cancelled
PendingApproval → Registered → Invoiced → Paid
PendingApproval → Rejected
```

### Invoice Lifecycle
```
Draft → PendingPayment → Finalized (Paid)
Draft → Cancelled
```

### Order Item with Addons
`OrderItem` supports `OrderItemAddon` children. When an order is registered, 
invoice items are populated from order items including addons.

## API Endpoints

### Order Registration (NEW - Unified)
```
POST /api/orders/register
Authorization: perm:orders.create
Content-Type: application/json

{
  "tableNumber": "5",
  "items": [
    {
      "menuItemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 2,
      "addons": [
        { "menuAddonId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "quantity": 1 }
      ]
    }
  ]
}
```

This endpoint:
1. Creates an Order with all items and addons (price snapshots)
2. Creates a Draft Invoice with items and addons populated from the order
3. Registers the order (moves to Registered)
4. Moves the invoice to PendingPayment (ready for payment)
5. Returns the order + invoice response

### Order Management
- `POST /api/orders` - Create POS draft order
- `POST /api/orders/{id}/items` - Add item (with optional addons)
- `DELETE /api/orders/{id}/items/{itemId}` - Remove item
- `POST /api/orders/{id}/register` - Register draft order (creates invoice items)
- `POST /api/orders/{id}/cancel` - Cancel order
- `PUT /api/orders/{id}/table-number` - Set table number
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/queue?status=Registered` - Get order queue

### External Orders
- `POST /api/external-orders` - Submit web order (pending approval)
- `POST /api/orders/{id}/approve` - Approve external order
- `POST /api/orders/{id}/reject` - Reject external order

### Invoice Management
- `POST /api/invoices` - Create standalone invoice (with addons)
- `GET /api/invoices/{id}` - Get invoice by ID
- `POST /api/invoices/{id}/pay` - Mark invoice as paid
- `POST /api/invoices/{id}/cancel` - Cancel invoice
- `POST /api/invoices/{id}/settle` - Finalize invoice (with inventory deduction)
- `POST /api/invoices/{id}/card-payment` - POS card payment

## Flow: Order Registration → Invoice → Payment

### Unified Flow (Recommended)
1. Frontend sends `POST /api/orders/register` with items + addons
2. Backend creates order, populates invoice, registers order
3. Invoice is in PendingPayment status
4. Payment can happen immediately via `POST /api/invoices/{id}/settle` or `POST /api/invoices/{id}/card-payment`
5. Or payment can be deferred - invoice stays pending until paid

### Legacy Flow (Backward Compatible)
1. `POST /api/orders` → creates draft order
2. `POST /api/orders/{id}/items` → adds items (with addons now supported)
3. `POST /api/orders/{id}/register` → registers order, populates invoice items, marks pending

## Code Conventions

- Domain entities use private constructors + static factory methods
- Repository interfaces live in `Domain/Repositories/`
- Application services use constructor injection
- Controllers are thin, delegating to application services
- Response DTOs are records in `Application/DTOs/`
- Mappers are extension methods in `Application/Mappers/`
- EF configuration is inline in `AppDbContext.OnModelCreating`
- Authorization uses policy-based permissions (e.g., `perm:orders.create`)