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

## Exact continuation for the next cycle

Continue objective B by first introducing the missing add-on catalog/order model (`MenuAddon` plus invoice add-on snapshots and applicable-menu relationships), then add an add-on recipe target that reuses ingredient consumption and extend `InvoiceService.FinalizeAsync` to deduct both menu-item and add-on BOM quantities. Add tests for insufficient stock, concurrent settlement, idempotent settlement, and recipe replacement. After that, proceed to objective C (ESC/POS kitchen/customer printing).
