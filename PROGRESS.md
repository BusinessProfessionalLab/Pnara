# Project Progress

## State at cycle start

- The solution has four projects: `Domain`, `Application`, `Infrastructure`, and `WebApi`, using .NET 10 and EF Core/Npgsql.
- Existing style: domain entities use private constructors and factory methods; repository interfaces live in `Domain`; application services live in `Application`; EF implementations live in `Infrastructure`; controllers are thin in `WebApi`.
- Existing capabilities: JWT/cookie authentication, `Admin`/`Operator`/`User` roles, user and role management, menu/group/image management, company information, invoice settlement, and sales reporting.
- No inventory, printing, terminal, or background-task model/service existed at cycle start.
- The existing project has no `MenuAddon`/add-on catalog or add-on selection in invoice requests.
- `Domain.Tests` contains build output only; no test project/source files were found.
- Git was initially unavailable on PATH, but a Visual Studio-bundled Git executable was found before the end of this cycle.

## Result of this cycle

- Added inventory domain entities:
  - `MeasurementUnit` for admin-managed units.
  - `Ingredient` with active state, current/minimum stock, and an optimistic concurrency token.
  - `StockLedgerEntry` for opening balances, manual adjustments, and invoice consumption.
  - `MenuItemRecipe`/`RecipeComponent` for menu-item BOM definitions.
- Added `IInventoryRepository` and its EF implementation, including unit/ingredient/ledger/recipe queries and concurrency-conflict translation.
- Added `InventoryService` and admin-only `GET/POST/PUT /api/inventory/...` endpoints for:
  - units;
  - ingredients and manual stock adjustments;
  - low-stock listing (`GET /api/inventory/low-stock`);
  - ingredient kardex;
  - replacing and reading menu-item recipes.
- Integrated inventory into `InvoiceService.FinalizeAsync`:
  - every menu item on an invoice must have a recipe;
  - recipe quantities are multiplied by invoice quantities and aggregated by ingredient;
  - insufficient stock prevents settlement;
  - ingredient balances, consumption ledger entries, and invoice finalization are persisted through one EF `SaveChangesAsync` transaction;
  - concurrent stock changes return HTTP 409 and do not finalize the invoice.
- Added migration `20260820002918_AddInventory` and updated the EF model snapshot.
- Completed the previously uncommitted invoice/reporting files from the prior cycle in the same coherent working tree; the current invoice service includes the inventory settlement boundary.

## Active priority

Objective A (invoice reporting) is complete for current invoice/channel/payment models. Objective B is partially complete: ingredient inventory, units, low-stock visibility, kardex, menu-item recipes, and transactional deduction are implemented. Add-on recipes are not yet wired because the project has no add-on domain/catalog or invoice add-on selection model; adding a generic recipe target without that order model would allow configurations that can never be consumed.

Customer ownership and external online payment verification remain intentionally unmodeled. Settlement is currently restricted to `AdminOrOperator`; inventory administration is restricted to `AdminOnly`.

## Validation and stability

- `dotnet build PinaraSolution.slnx --no-restore`: passed with 0 warnings and 0 errors after each completed implementation section and at cycle end.
- `dotnet test PinaraSolution.slnx --no-restore`: passed; the solution contains no runnable test project.
- `dotnet ef migrations script --idempotent --project Infrastructure --startup-project WebApi`: passed for the complete migration chain.
- No database integration test was run because no test project or configured test database exists.
- A Visual Studio-bundled Git executable is available; commit creation is pending the final staged-tree review.

## Result of this cycle (continued)

- Completed the missing add-on catalog/order model for Objective B:
  - `MenuAddon` supports name, description, price, availability, and display order.
  - `MenuAddonMenuItem` stores which add-ons are valid for each menu item.
  - `InvoiceItemAddon` stores an immutable invoice snapshot of add-on name, unit price, quantity, and line total.
  - Public menu responses now expose available add-ons per available menu item.
- Added admin-managed add-on APIs:
  - `POST/PUT/PATCH/GET /api/menu/addons...` for catalog and availability.
  - `PUT /api/menu/addons/{id}/applicability` for replacing applicable menu items.
- Added add-on BOM entities, repository methods, service methods, and admin-only APIs:
  - `PUT/GET /api/inventory/recipes/menu-addons/{menuAddonId}`.
- Extended invoice creation:
  - validates add-on existence, availability, and applicability to the selected menu item;
  - groups equivalent menu-item/add-on selections;
  - snapshots add-on details into `InvoiceItem`.
- Extended invoice finalization:
  - loads add-on snapshots with the invoice;
  - requires a recipe for every selected add-on;
  - aggregates menu-item and add-on ingredient requirements together;
  - deducts the combined requirement and writes the same invoice-linked kardex entries inside the existing atomic `SaveChangesAsync` boundary.
- Added EF migration `20260820054032_AddMenuAddons` and updated `AppDbContextModelSnapshot`.

## Files added or changed this cycle

- Domain: `MenuAddon.cs`, `MenuAddonMenuItem.cs`, `InvoiceItemAddon.cs`, `MenuAddonRecipe.cs`, `MenuAddonRecipeComponent.cs`, `IMenuAddonRepository.cs`, plus `InvoiceItem.cs` and `IInventoryRepository.cs`.
- Application: `MenuAddonRequests.cs`, `MenuAddonResponses.cs`, `MenuAddonMapper.cs`, `MenuAddonService.cs`, plus invoice/public-menu DTOs, mappers, `InvoiceService.cs`, `InventoryService.cs`, and `MenuGroupService.cs`.
- Infrastructure: `MenuAddonRepository.cs`, `20260820054032_AddMenuAddons.cs`, its designer, snapshot, DbContext mappings, DI registration, inventory repository, and invoice repository eager-loading.
- WebApi: menu/add-on and inventory recipe endpoints, service registrations, and menu controller dependencies.

## Validation and stability (this cycle)

- `dotnet build PinaraSolution.slnx --no-restore`: passed with 0 warnings and 0 errors.
- `dotnet test PinaraSolution.slnx --no-restore`: passed; the solution contains no runnable test project.
- `dotnet ef migrations script --idempotent --project Infrastructure --startup-project WebApi`: passed and produced a migration script.
- No database integration test was run because no test project or configured test database exists.

## Result of this cycle (Objective C - printing core)

- Added printer and receipt configuration entities:
  - `PrinterDefinition` with TCP connection settings, active state, and 58/80 mm paper width.
  - `ReceiptTemplate` for kitchen/customer-specific header/footer, logo/pricing/tax/payment/channel visibility, font size, and active state.
  - `ReceiptPrinterMapping` for one printer per receipt type.
- Added `IReceiptPrinterClient` and an Infrastructure ESC/POS TCP adapter.
- Added configurable receipt renderer:
  - kitchen receipt hides prices and includes order number, channel, time, items, and add-ons;
  - customer receipt supports company name/logo flag, item/add-on prices, discount, tax, total, payment method, and footer;
  - width-aware output for 58 mm and 80 mm paper;
  - ESC/POS initialize, line feed, and cut commands.
- Added admin APIs:
  - printer CRUD;
  - kitchen/customer template upsert and listing;
  - receipt-type-to-printer mapping.
- Added operator/admin invoice printing API for customer or kitchen receipts.
- Integrated automatic kitchen printing after successful invoice settlement.
  - Printing is best-effort: a printer/network failure does not roll back a durable invoice or inventory transaction.
  - Failed prints can be retried through the print endpoint.
- Added default kitchen/customer templates during database seeding.
- Added migration `20260820065250_AddReceiptPrinting` and updated the EF model snapshot.

## Files added or changed this cycle

- Domain: `ReceiptType.cs`, `PrinterConnectionType.cs`, `PrinterDefinition.cs`, `ReceiptTemplate.cs`, `ReceiptPrinterMapping.cs`, and `IPrintingRepository.cs`.
- Application: printing DTOs, mappers, `IReceiptPrinterClient`, `IReceiptPrintingService`, `PrintingException`, `ReceiptPrintingService`, `EscPosReceiptRenderer`, and invoice settlement integration.
- Infrastructure: `PrintingRepository`, `EscPosTcpPrinterClient`, EF mappings, DI registration, migration/designer, and snapshot.
- WebApi: `PrintingController`, printing exception mapping, and service registration.
- `PROGRESS.md`.

## Validation and stability (this cycle)

- `dotnet build PinaraSolution.slnx --no-restore`: passed with 0 warnings and 0 errors after final changes.
- `dotnet test PinaraSolution.slnx --no-restore`: passed; the solution contains no runnable test project.
- `dotnet ef migrations script --idempotent --project Infrastructure --startup-project WebApi`: passed and produced a complete script including the printing migration.
- No database integration or physical-printer test was run because no configured test database or printer exists.

## Known scope and next priority

- The current printer adapter supports TCP ESC/POS. The interface and connection enum leave room for a serial adapter without changing Application services; serial support remains TODO.
- Receipt templates currently expose the configurable fields represented by the existing invoice model. Table number, notes, customer identity, and logo bitmap rendering are not modeled by the current invoice/company entities and remain TODO.
- Objective B remains complete. Continue Objective C with serial/bitmap printer support and richer order metadata if those models are introduced; then proceed to Objective D (PC-POS adapter) and Objective E (background services).
