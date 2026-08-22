# مستندات کامل API های پینارا (Pinara API)

نرم‌افزار مدیریت سفارش کافه/رستوران — شامل صندوق فروش (POS)، سفارش آنلاین، منو، انبار، چاپ رسید و گزارش فروش.

- Base URL محلی: `https://localhost:5002` یا `https://192.168.100.17:5002`
- Swagger UI: `/swagger` (در حالت Development فعال می‌شود)
- فرمت همه‌ی Request/Response ها JSON است (به‌جز ساخت/آپدیت آیتم منو که `multipart/form-data` است، نه `application/json`).
- راه‌اندازی با Docker Compose یکجا: `docker compose up -d` (PostgreSQL + WebAPI)
- دانش‌باش‌های پیش‌فرض: `POST /api/auth/login` → ایمیل `admin@pinara.com` / پسورد `Admin@12345`

---

## ۰. نتیجهٔ تست عملیاتی (Run against live container)

پروژه در تاریخ **۲۲ اوت ۲۰۲۶** بررسی شد:

| چک‌اوت | وضعیت | یادداشت |
|---|---|---|
| `dotnet build` | ✅ موفق | 2 warning، 0 خطا |
| `dotnet test` (Domain.Tests) | ✅ ۷۳/۷۳ Passed | 58ms |
| Docker Compose بالا آمد | ✅ | PostgreSQL 16 + ASP.NET 10 WebAPI |
| `GET /api/company-info` (anonymous) | ✅ کار می‌کند | |
| `POST /api/auth/login` (admin) | ✅ کار می‌کند | توکن JWT در کوکی `accessToken` + `refreshToken` |
| `GET /api/menu` (public) | ✅ کار می‌کند | |
| `POST /api/menu/groups` | ✅ کار می‌کند | AdminOnly |
| `POST /api/menu/groups/{id}/items` | ✅ کار می‌کند | **multipart/form-data**، نه JSON |
| `POST /api/inventory/units` | ✅ کار می‌کند | فیلدها: `name`, `symbol` |
| `POST /api/inventory/ingredients` | ✅ کار می‌کند | فیلدها: `measurementUnitId`, `name`, `openingStock`, `minimumStock` |
| `PUT /api/inventory/recipes/menu-items/{id}` | ✅ کار می‌کند | |
| `POST /api/orders` | ✅ کار می‌کند | Draft ساخته می‌شود، `orderId` + `invoiceId` برمی‌گردد |
| `POST /api/orders/{id}/items` | ✅ کار می‌کند | |
| `PUT /api/orders/{id}/table-number` | ✅ کار می‌کند | |
| `POST /api/orders/{id}/register` | ✅ فراخوانی می‌شود | **اما جزئیات در بخش ۶/باگ ۸ ببینید** |
| `POST /api/orders/{id}/cancel` | ✅ کار می‌کند | 204 NoContent |
| `GET /api/orders/queue` | ✅ کار می‌کند | |
| `GET /api/orders/{id}` | ✅ کار می‌کند | |
| `GET /api/reports/sales` | ✅ کار می‌کند | |
| `GET /api/inventory/ingredients` | ✅ کار می‌کند | |
| `GET /api/inventory/ingredients/{id}/ledger` | ✅ کار می‌کند | |
| `POST /api/invoices/{id}/settle` | ⚠️ شکست می‌خورد | **باگ ۸** — فاکتور آیتم ندارد |
| `POST /api/auth/register` (customer) | ❌ ۵۰۰ می‌دهد | **باگ ۹** — نقش User سید نشدته |

> نتیجهٔ تست: **۲ باگ بحرانی** کشف شد که جریان فروش کامل POS و ثبت‌نام مشتری آنلاین را خراب می‌کنند (بخش ۶/باگ ۸ و ۹). بقیهٔ endpointها بدون مشکل کار می‌کنند.

---

## ۱. احراز هویت و مدل دسترسی

احراز هویت با **JWT** انجام می‌شود. توکن access در هدر `Authorization: Bearer <token>` و refresh token در کوکی `refreshToken` ارسال/دریافت می‌شود. در Login، هر دو توکن به‌صورت کوکی (`accessToken` و `refreshToken`) هم ست می‌شوند تا مرورگر بدون هدر کار کند.

### سیاست‌های دسترسی (Authorization Policies)

| سیاست | شرط | کاربرد |
|---|---|---|
| بدون احراز (AllowAnonymous) | — | لاگین/ثبت‌نام، منوی عمومی، اطلاعات شرکت |
| `[Authorize]` | فقط کاربر واردشده | پروفایل، آدرس‌ها، ثبت سفارش وب |
| `AdminOnly` | claim `role == Admin` | مدیریت کاربران/نقش‌ها/پرمیشن‌ها، انبار، پرینتر، ترمینال POS |
| `AdminOrOperator` | `role ∈ {Admin, Operator}` | فاکتورها، پرداخت کارتخوان، گزارش فروش، چاپ رسید |
| `perm:<name>` | داشتن claim `permission` با همان نام **یا** نقش Admin | دسترسی ریزتر (مثل `perm:orders.create`) |

> پرمیشن‌ها در جدول `Permissions` نگهداری می‌شوند و از طریق نقش‌ها به کاربران می‌رسند. توکن JWT شامل claim های `role` و `permission` است.

---

## ۲. جریان‌های اصلی کسب‌وکار

### ۲.۱. جریان صندوق (POS) — طراحی فعلی

الگوی فعلی «Draft-first» است: اول یک پیش‌نویس خالی ساخته می‌شود، بعد آیتم‌ها یکی‌یکی اضافه می‌شوند:

```
POST /api/orders                        → ساخت Order(پیش‌نویس) + فاکتور Draft پیوسته به آن
POST /api/orders/{id}/items             → افزودن آیتم (یکی‌یکی)          ← بدنه دارد
DELETE /api/orders/{id}/items/{itemId}  → حذف آیتم
PUT  /api/orders/{id}/table-number      → شماره میز
POST /api/orders/{id}/register          → ثبت نهایی سفارش؛ قیمت‌ها اسنپ‌شات و
                                          فاکتور با مالیات شرکت محاسبه می‌شود
POST /api/invoices/{invoiceId}/pay      → پرداخت (فاکتور Finalized می‌شود)
POST /api/invoices/{invoiceId}/settle   → تسویه با ذکر روش پرداخت + کسر انبار + چاپ آشپزخانه
```

نکته مهم: **با ساخت سفارش POS، یک فاکتور Draft هم خودکار ساخته می‌شود** و `orderId` و اطلاعات فاکتور در پاسخ برمی‌گردد. فرانت باید از همین فاکتور استفاده کند؛ نباید `POST /api/invoices` را جداگانه صدا بزند (بخش ۶ را ببینید).

### ۲.۲. جریان سفارش آنلاین (وب/اپ مشتری)

```
GET  /api/menu                          → منوی عمومی (بدون لاگین)
POST /api/auth/register , /login        → ثبت‌نام/ورود مشتری
POST /api/user/addresses                → ثبت آدرس
POST /api/external-orders               → ثبت سفارش کامل با آیتم‌ها + AddressId (بدنه دارد)
POST /api/orders/{id}/approve           → تایید اپراتور → فاکتور محاسبه و آماده پرداخت
POST /api/orders/{id}/reject            → رد سفارش (با دلیل)
```

---

## ۳. وضعیت‌ها (State Machines)

### OrderStatus
`Draft → Registered → (Paid)` و برای وب: `Draft → PendingApproval → Registered/Rejected`

| وضعیت | معنا |
|---|---|
| `Draft` | در حال سفارش‌گیری؛ آیتم قابل افزودن/حذف |
| `PendingApproval` | سفارش وب منتظر تایید اپراتور |
| `Registered` | ثبت شده؛ فاکتور محاسبه شده |
| `Invoiced` / `Paid` | تعریف شده‌اند ولی هنوز در هیچ سرویسی ست نمی‌شوند ⚠️ |
| `Cancelled` / `Rejected` | لغو/رد |

### InvoiceStatus
`Draft → Finalized` یا `Draft → Cancelled`

| وضعیت | معنا |
|---|---|
| `Draft` | پیش‌نویس؛ همراه سفارش، مبلغ‌ها با هر تغییر آیتم به‌روز می‌شود |
| `Finalized` | قطعی/پرداخت‌شده؛ دیگر قابل تغییر نیست |
| `Cancelled` | لغو شده |

> ⚠️ وضعیت «در انتظار پرداخت» (PendingPayment) در دیتابیس وجود ندارد. متد `Invoice.MarkPendingPayment()` فعلاً **بدنه خالی** دارد و هیچ چیزی را تغییر نمی‌دهد (بخش ۶).

---

## ۴. مرجع کامل Endpoint ها

### ۴.۱. Auth — `AuthController` (`/api/auth`)
مدیریت هویت کاربران. پایه‌ی همه‌ی عملیات‌هاست.

| متد | مسیر | دسترسی | بدنه | کارکرد |
|---|---|---|---|---|
| POST | `/register` | عمومی | `RegisterRequest` | ثبت‌نام مشتری با نقش User |
| POST | `/login` | عمومی | `LoginRequest` | ورود و صدور JWT + کوکی‌ها |
| POST | `/refresh` | کوکی refreshToken | — | تمدید access token |
| POST | `/logout` | واردشده | — | ابطال refresh token و پاک کردن کوکی‌ها |

### ۴.۲. Orders — `OrdersController` (`/api/orders`)
هسته‌ی سفارش‌گیری صندوق و صف آشپزخانه.

| متد | مسیر | دسترسی | بدنه | کارکرد |
|---|---|---|---|---|
| POST | `/` | `perm:orders.create` | **ندارد (عمداً)** | ساخت پیش‌نویس خالی + فاکتور Draft خودکار |
| POST | `/{id}/items` | `perm:orders.create` | `{ menuItemId, quantity }` | افزودن آیتم به پیش‌نویس |
| DELETE | `/{id}/items/{itemId}` | `perm:orders.create` | — | حذف آیتم |
| PUT | `/{id}/table-number` | `perm:orders.create` | `{ tableNumber }` | تعیین/تغییر میز |
| POST | `/{id}/cancel` | `perm:orders.create` | — | لغو سفارش و فاکتور Draft آن |
| POST | `/{id}/register` | `perm:orders.create` | — | ثبت سفارش؛ اسنپ‌شات قیمت + محاسبه مالیات |
| GET | `/queue?status=` | `perm:orders.view` | — | صف سفارش‌ها (پیش‌فرض: Registered) — نمایشگر آشپزخانه |
| GET | `/{id}` | `perm:orders.view` | — | جزئیات یک سفارش + فاکتور آن |
| POST | `/{id}/approve` | `perm:orders.approve` | — | تایید سفارش وب توسط اپراتور |
| POST | `/{id}/reject` | `perm:orders.approve` | `{ reason }` | رد سفارش وب با ذکر دلیل |

**چرا `POST /api/orders` بدنه ندارد؟** چون طراحی فعلی «اول پیش‌نویس، بعد آیتم‌به‌آیتم» است. کلاینت پس از ساخت، `id` را از پاسخ می‌گیرد و با `/items` آیتم اضافه می‌کند. (DTO آماده‌ی `CreatePosOrderRequest { tableNumber }` در پروژه وجود دارد ولی به کنترلر وصل نشده — یعنی قرار بوده بدنه داشته باشد.)

### ۴.۳. External Orders — `ExternalOrdersController` (`/api/external-orders`)
ثبت سفارش اینترنتی مشتری در **یک درخواست** (برخلاف POS):

| متد | مسیر | دسترسی | بدنه | کارکرد |
|---|---|---|---|---|
| POST | `/` | واردشده | `{ items[], addressId }` | ساخت سفارش کامل PendingApproval + فاکتور Draft |

### ۴.۴. Invoices — `InvoicesController` (`/api/invoices`)
چرخه حیات فاکتور. سطح دسترسی پایه: `AdminOrOperator`.

| متد | مسیر | دسترسی | بدنه | کارکرد |
|---|---|---|---|---|
| POST | `/` | AdminOrOperator | `{ channel, items[{ menuItemId, quantity, addons[] }], discountAmount?, taxAmount? }` | ساخت فاکتور **مستقل** (بدون اتصال به سفارش!) |
| POST | `/{id}/pay` | `perm:invoices.pay` | — | پرداخت؛ فاکتور Finalized با روش Card ثبت می‌شود |
| POST | `/{id}/cancel` | `perm:invoices.cancel` | — | لغو فاکتورِ غیرقطعی |
| GET | `/{id}` | `perm:invoices.view` | — | دریافت فاکتور با آیتم‌ها و جمع‌ها |
| POST | `/{id}/settle` | AdminOrOperator | `{ paymentMethod: Cash/Card/Online }` | **تسویه نهایی**: قطعی‌سازی + کسر مواد از انبار (رسپی اجباری) + ارسال رسید آشپزخانه |

### ۴.۵. POS Payments — `PosPaymentsController` (`/api/invoices`)
اتصال به کارتخوان فیزیکی (ترمینال TCP). سطح: `AdminOrOperator`.

| متد | مسیر | کارکرد |
|---|---|---|
| POST | `/{id}/card-payment` | شروع پرداخت کارتی روی دستگاه؛ در موفقیت، فاکتور خودکار Settle می‌شود. خطاهای Cancelled/TimedOut/Unknown وضعیت‌گذاری و ذخیره می‌شوند |

### ۴.۶. Menu — `MenuController` (`/api/menu`) + `PublicMenuController`
مدیریت منو (گروه‌ها، آیتم‌ها، افزودنی‌ها). سطح: `perm:manage`.

| متد | مسیر | کارکرد |
|---|---|---|
| POST | `/groups` | ساخت گروه منو |
| PUT | `/groups/{id}` | ویرایش گروه |
| PATCH | `/groups/{id}/status` | فعال/غیرفعال کردن گروه |
| GET | `/groups?includeInactive=` | لیست گروه‌ها |
| POST | `/groups/{groupId}/items` | ساخت آیتم منو (+ آپلود همزمان تصویر، multipart) |
| PUT | `/items/{id}` | ویرایش آیتم |
| PATCH | `/items/{id}/status` | فعال/غیرفعال |
| PATCH | `/items/{id}/availability` | موجود/ناموجود کردن (مخصوص فروش) |
| POST | `/items/{id}/image` ، DELETE `/items/{id}/image` | آپلود/حذف تصویر |
| GET | `/groups/{groupId}/items` | آیتم‌های یک گروه |
| POST | `/addons` ، PUT `/addons/{id}` | مدیریت افزودنی‌ها (Admin) |
| PATCH | `/addons/{id}/availability` | موجود/ناموجود افزودنی (Admin) |
| GET | `/addons?includeUnavailable=&menuItemId=` | لیست افزودنی‌ها |
| PUT | `/addons/{id}/applicability` | تعیین اینکه افزودنی به کدام آیتم‌ها می‌خورد (Admin) |
| GET | `/api/menu` (PublicMenuController) | **منوی عمومی بدون لاگین** برای سایت/اپ مشتری |

### ۴.۷. Modifier Groups — `ModifierController` (`/api/modifier-groups`)
گروه‌های انتخابی روی آیتم منو (سایز، شیرینی، ...). سطح: `perm:manage`.

| متد | مسیر | کارکرد |
|---|---|---|
| POST | `/` | ساخت گروه |
| GET | `/` ، `/{id}` | لیست/جزئیات |
| PUT | `/{id}` | ویرایش گروه |
| POST | `/{groupId}/modifiers` | افزودن گزینه به گروه |
| PUT | `/{groupId}/modifiers/{modifierId}` | ویرایش گزینه |
| DELETE | `/{groupId}/modifiers/{modifierId}` | حذف گزینه |
| PATCH | `/{groupId}/modifiers/{modifierId}/availability` | موجود/ناموجود گزینه |
| POST / DELETE | `/{groupId}/menu-items/{menuItemId}` | اتصال/جداسازی گروه از آیتم منو |
| GET | `/menu-items/{menuItemId}` | گروه‌های متصل به یک آیتم |

### ۴.۸. Users / Roles / Permissions
مدیریت کارکنان و سطوح دسترسی.

**Users — `/api/users`:**

| متد | مسیر | دسترسی | کارکرد |
|---|---|---|---|
| GET | `/me` | واردشده | پروفایل کاربر جاری |
| PUT | `/{id}/role` | Admin | تغییر نقش کاربر |

**Admin — `/api/admin` (Admin):**

| متد | مسیر | کارکرد |
|---|---|---|
| POST | `/users` | ساخت کاربر توسط ادمین (کارکنان) |
| GET | `/users?roleId=` | لیست کاربران با فیلتر نقش |

**Roles — `/api/roles` (Admin):**

| متد | مسیر | کارکرد |
|---|---|---|
| POST / GET | `/` | ساخت / لیست نقش‌ها |
| GET / PUT / DELETE | `/{id}` | جزئیات / ویرایش / حذف |
| PUT | `/{roleId}/permissions` | جایگزینی کامل پرمیشن‌های نقش |
| DELETE | `/{roleId}/permissions/{permissionId}` | گرفتن یک پرمیشن |
| GET | `/{roleId}/permissions` | پرمیشن‌های نقش |

**Permissions — `/api/permissions` (Admin):**

| متد | مسیر | کارکرد |
|---|---|---|
| GET | `/` | لیست همه پرمیشن‌ها |
| POST | `/` | ساخت پرمیشن سفارشی |
| DELETE | `/{id}` | حذف پرمیشن غیرسیستمی |

### ۴.۹. Company Info — `CompanyInfoController` (`/api/company-info`)
اطلاعات رستوران (نام، لوگو، مالیات، تاریخ نصب برای دوره آزمایشی).

| متد | مسیر | دسترسی | کارکرد |
|---|---|---|---|
| GET | `/` | عمومی | نام/لوگو برای نمایش در منوی عمومی و رسید |
| PATCH | `/tax-settings` | `perm:settings.manage` | فعال‌سازی مالیات و نرخ آن (در Register/Settle اعمال می‌شود) |

### ۴.۱۰. Inventory — `InventoryController` (`/api/inventory`) — فقط Admin
مدیریت مواد اولیه و رسپی‌ها. **پیش‌نیاز Settle:** هر آیتم/افزودنیِ داخل فاکتور باید رسپی داشته باشد وگرنه تسویه خطا می‌دهد.

| متد | مسیر | کارکرد |
|---|---|---|
| POST / PUT / GET | `/units...` | واحدهای اندازه‌گیری |
| POST / PUT / GET | `/ingredients...` | مواد اولیه با موجودی فعلی/حداقل |
| GET | `/low-stock` | مواد نزدیک به اتمام |
| POST | `/ingredients/{id}/adjustments` | اصلاح دستی موجودی (رسید/ضایعات) |
| GET | `/ingredients/{id}/ledger?fromUtc=&toUtc=` | کاردکس ماده (تاریخچه ورود/مصرف) |
| PUT / GET | `/recipes/menu-items/{menuItemId}` | جایگزینی/مشاهده رسپی آیتم منو |
| PUT / GET | `/recipes/menu-addons/{menuAddonId}` | جایگزینی/مشاهده رسپی افزودنی |

### ۴.۱۱. Printing — `PrintingController` (`/api/printing`)
مدیریت پرینترهای حرارتی شبکه (ESC/POS) و قالب رسیدها.

| متد | مسیر | دسترسی | کارکرد |
|---|---|---|---|
| GET / POST / PUT | `/printers...` | Admin | مدیریت پرینترها (IP/Port) |
| GET | `/templates` | Admin | قالب‌های رسید (آشپزخانه/مشتری) |
| PUT | `/templates/{receiptType}` | Admin | ویرایش قالب (هدر/فوتر/فونت/نمایش قیمت و...) |
| GET | `/mappings` | Admin | نگاشت نوع رسید → پرینتر |
| PUT | `/mappings/{receiptType}` | Admin | نسبت دادن پرینتر به نوع رسید |
| POST | `/invoices/{invoiceId}/{receiptType}` | AdminOrOperator | چاپ رسید یک فاکتور |

### ۴.۱۲. POS Terminals — `PosTerminalsController` (`/api/pos-terminals`) — فقط Admin
مدیریت کارتخوان‌های متصل (TCP).

| متد | مسیر | کارکرد |
|---|---|---|
| GET / POST | `/` | لیست / ثبت ترمینال |
| PUT / DELETE | `/{id}` | ویرایش / حذف |

### ۴.۱۳. Reports — `ReportsController` (`/api/reports`) — AdminOrOperator

| متد | مسیر | کارکرد |
|---|---|---|
| GET | `/sales?fromUtc=&toUtc=&channel=&paymentMethod=&top=` | گزارش فروش بازه‌ای + پرفروش‌ترین‌ها (فقط فاکتورهای Finalized) |

### ۴.۱۴. User Addresses — `UserAddressesController` (`/api/user/addresses`)
آدرس‌های مشتری برای سفارش آنلاین (CRUD + تعیین آدرس پیش‌فرض). فقط آدرس‌های خودِ کاربر واردشده.

---

## ۵. سناریوی نمونه: یک فروش کامل در صندوق

```http
POST /api/auth/login                      → ورود کَشیر
POST /api/orders                          → { orderId, invoiceId } (پیش‌نویس)
POST /api/orders/{orderId}/items          → { "menuItemId": "...", "quantity": 2 }
POST /api/orders/{orderId}/items          → ...
POST /api/orders/{orderId}/register       → سفارش Registered؛ فاکتور با قیمت/مالیات نهایی
POST /api/invoices/{invoiceId}/settle     → { "paymentMethod": 1 }  (Cash)
                                          → فاکتور Finalized + کسر انبار + چاپ آشپزخانه
```

یا برای پرداخت کارتی، به‌جای settle:
```http
POST /api/invoices/{invoiceId}/card-payment
```

---

## ۶. مشکلات و نواقص کشف‌شده در بازبینی (مهم!)

1. **وضعیت «در انتظار پرداخت» واقعاً وجود ندارد.**
   `Invoice.MarkPendingPayment()` در `Domain/Entities/Invoice.cs:115` بدنه‌اش خالی است؛ یعنی بعد از `register` یا `approve`، فاکتور همچنان `Draft` می‌ماند و هیچ لیستی به‌نام «در انتظار پرداخت‌ها» قابل ساخت نیست. برای تحقق خواسته‌تان باید enum `InvoiceStatus` مقدار `PendingPayment` بگیرد، این متد واقعاً وضعیت را تغییر دهد، و endpoint لیست (بند ۳) اضافه شود.

2. **`POST /api/invoices` فاکتور بی‌ربط به سفارش می‌سازد.**
   این endpoint هیچ `OrderId` نمی‌گیرد و فاکتور مستقل ایجاد می‌کند. اگر فرانت بعد از ثبت سفارش این را صدا بزند، دو رشته‌ی جدا از هم دارید. فاکتورِ سفارش همان است که موقع `POST /api/orders` خودکار ساخته شده.

3. **لیست «در انتظار پرداخت‌ها» وجود ندارد.**
   `InvoicesController` فقط GetById دارد؛ برای صفحه‌ی صندوق باید `GET /api/invoices?status=...` (یا معادلش) اضافه شود.

4. **`pay` همیشه روش پرداخت را Card ثبت می‌کند.**
   `Invoice.Pay()` در `Invoice.cs:161` با `PaymentMethod.Card` قطعی می‌شود؛ برای فروش نقدی از `/settle` با `Cash` استفاده کنید یا `pay` را اصلاح کنید.

5. **Settle بدون رسپی انبار خطا می‌دهد.**
   اگر حتی یک آیتم/افزودنیِ فاکتور رسپی نداشته باشد، تسویه با DomainException شکست می‌خورد. قبل از فروش واقعی، رسپی‌ها را در `/api/inventory/recipes/...` تنظیم کنید.

6. **`AdminOrOperator` قبلاً ثبت نشده بود** — باگ بود و اصلاح شد (Program.cs). چهار کنترلر فاکتور/پرداخت/گزارش/چاپ عملاً کرش می‌کردند.

7. **`CreatePosOrderRequest { tableNumber }` بلااستفاده است** — نشانه‌ی این است که قرار بوده `POST /api/orders` بدنه بپذیرد (شماره میز) ولی وصل نشده. برای تنظیم میز، از `PUT /api/orders/{id}/table-number` با بدنه `{ "tableNumber": "5" }` استفاده کنید.

8. **🟥 باگ بحرانی: آیتم‌های فاکتور (Invoice) هرگز از سفارش ساخته نمی‌شوند.**
   در `OrderService.AddItemAsync` (`Application/Services/OrderService.cs:42-60`) آیتم به **سفارش** اضافه می‌شود اما هیچ‌وقت به **فاکتور** اضافه نمی‌شود. متد `RecalculateDraftInvoiceAsync` مجموع فاکتور را از لیست خالی `_items` محاسبه می‌کند؛ یعنی `Subtotal` همیشه ۰ می‌ماند. در نتیجه:
   - موقع `register` یا `settle`، فاکتور مقدار ۰ دارد.
   - متد `Invoice.Finalize()` در `Domain/Entities/Invoice.cs:181` با خطای `DomainException("An invoice must contain at least one item.")` شکست می‌خورد و **تسویه (settle) هرگز موفق نمی‌شود.**
   - به زبان دیگر: **جریان فروش نقدی صندوق کاملاً غیرقابل استفاده است تا زمانی که این باگ رفع نشود.**
   
   راه‌حل پیشنهادی: در `AddItemAsync`، علاوه بر `order.AddItem(...)`، باید `invoice.AddItem(InvoiceItem.Create(menuItem.Id, menuItem.Name, request.Quantity, menuItem.Price))` هم صدا بزنید و سپس `SaveChanges` کنید تا فاکتور بتواند subtotal صحیح داشته باشد. در تست عملیاتی بالا، `settle` با خطای "An invoice must contain at least one item" شکست خورد.

9. **🟥 باگ بحرانی: نقش‌های `User` و `Operator` در seed ساخته نمی‌شوند.**
   `DatabaseSeeder.SeedRolesAsync` (`Infrastructure/Seeding/DatabaseSeeder.cs:60-88`) فقط نقش **Admin** را می‌سازد. برای `User` و `Operator` فقط بررسی می‌کند اگر موجود باشند، ولی هرگز ساخته نمی‌شوند. درنتیجه:
   - `POST /api/auth/register` (ثبت‌نام مشتری) همیشه با `InvalidOperationException: Default 'User' role not found` → **500 Internal Server Error** شکست می‌خورد.
   - به معنای این است که **ثبت‌نام مشتری آنلاین و سفارشات وب خراب هستند.**
   - `POST /api/admin/users` هم نمی‌تواند کاربری با نقش User یا Operator بسازد، زیرا این نقش‌ها وجود ندارند.
   
   راه‌حل پیشنهادی: در `SeedRolesAsync`، نقش‌های `User` و `Operator` را همانند Admin بسازید (حداقل `User` برای ثبت‌نام مشتری).

---

## ۷. مثال‌های کاربردی cURL (جریان صندوق کامل — تست‌شده)

> نکته: فرانت می‌تواند از کوکی‌های `accessToken`/`refreshToken` استفاده کند (با `credentials: 'include'` در fetch) یا توکن JWT را از کوکی بخواند و در هدر `Authorization: Bearer` بفرستد. در مثال‌های زیر از کوکی‌جر استفاده شده (`-b admin.txt`).

**۰. لاگین:**
```bash
curl -sk -X POST https://localhost:5002/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@pinara.com","password":"Admin@12345"}' \
  -c admin.txt -D headers.txt
```
توکن JWT در کوکی `accessToken` قرار می‌گیرد.

**۱. ساخت گروه منو:**
```bash
curl -sk -X POST https://localhost:5002/api/menu/groups \
  -b admin.txt \
  -H "Content-Type: application/json" \
  -d '{"name":"غذای اصلی","displayOrder":1,"isInactive":false}'
```

**۲. ساخت آیتم منو (⚠️ multipart/form-data، نه JSON):**
```bash
curl -sk -X POST https://localhost:5002/api/menu/groups/{groupId}/items \
  -b admin.txt \
  -F "name=برجر" -F "price=250000" -F "groupId={groupId}" -F "isInactive=false"
```

**۳. ساخت یونیت + ماده اولیه + رسپی:**
```bash
curl -sk -X POST https://localhost:5002/api/inventory/units \
  -b admin.txt -H "Content-Type: application/json" \
  -d '{"name":"gram","symbol":"g"}'

curl -sk -X POST https://localhost:5002/api/inventory/ingredients \
  -b admin.txt -H "Content-Type: application/json" \
  -d '{"measurementUnitId":"{unitId}","name":"beef","openingStock":5000,"minimumStock":500}'

curl -sk -X PUT https://localhost:5002/api/inventory/recipes/menu-items/{menuItemId} \
  -b admin.txt -H "Content-Type: application/json" \
  -d '{"components":[{"ingredientId":"{ingredientId}","quantity":200}]}'
```

**۴. ساخت سفارش صندوق (Draft) + افزودن آیتم + میز:**
```bash
# سفارش Draft + فاکتور Draft اتوماتیک
curl -sk -X POST https://localhost:5002/api/orders -b admin.txt
# => {"id":"{orderId}","invoice":{"id":"{invoiceId}","status":"Draft",...}}

# افزودن ۲ عدد برجر
curl -sk -X POST https://localhost:5002/api/orders/{orderId}/items \
  -b admin.txt -H "Content-Type: application/json" \
  -d '{"menuItemId":"{menuItemId}","quantity":2}'

# شماره میز
curl -sk -X PUT https://localhost:5002/api/orders/{orderId}/table-number \
  -b admin.txt -H "Content-Type: application/json" \
  -d '{"tableNumber":"5"}'
```

**۵. ثبت و تسویه (settle) — ⚠️ در حال حاضر شکست می‌خورد (باگ ۸):**
```bash
curl -sk -X POST https://localhost:5002/api/orders/{orderId}/register -b admin.txt
curl -sk -X POST https://localhost:5002/api/invoices/{invoiceId}/settle \
  -b admin.txt -H "Content-Type: application/json" \
  -d '{"paymentMethod":1}'    # 1=Cash, 2=Card, 3=Online
```
> هم‌اکنون /settle با خطا `"An invoice must contain at least one item."` شکست می‌خورد (باک ۸). پس از رفع آن، این جریان کامل خواهد شد.

**۶. دریافت صف و فاکتور:**
```bash
curl -sk https://localhost:5002/api/orders/queue -b admin.txt
curl -sk https://localhost:5002/api/orders/{orderId} -b admin.txt
curl -sk https://localhost:5002/api/invoices/{invoiceId} -b admin.txt
```

**۷. منو و اطلاعات شرکت (بدون احراز هویت):**
```bash
curl -sk https://localhost:5002/api/menu        # منوی عمومی برای سایت/اپ مشتری
curl -sk https://localhost:5002/api/company-info  # نام/لوگو/مالیات
```

---

## ۸. مرجع خلاصهٔ DTO ها

| DTO | فیلدها |
|---|---|
| `RegisterRequest` | email، password (min 8)، firstName، lastName |
| `LoginRequest` | email، password |
| `CreateMenuGroupRequest` | name، description؟، displayOrder، isInactive |
| `CreateMenuItemRequest` | groupId، name، description؟، price، imageUrl؟، displayOrder — **از طریق form-data** |
| `AddOrderItemRequest` | menuItemId (Guid)، quantity (int 1-1000) |
| `SetTableNumberRequest` | tableNumber (string) |
| `CreateMeasurementUnitRequest` | name، symbol |
| `CreateIngredientRequest` | measurementUnitId، name، openingStock (پیش‌فرض ۰)، minimumStock (پیش‌فرض ۰) |
| `RecipeComponentRequest` | ingredientId، quantity (>= 0.001) |
| `CreateInvoiceRequest` | channel (Online/InPerson)، items[]، discountAmount؟، taxAmount؟ |
| `FinalizeInvoiceRequest` | paymentMethod (1=Cash / 2=Card / 3=Online) |
| `SubmitExternalOrderRequest` | items[] (menuItemId، quantity)، addressId |
| `CreateUserRequest` (Admin) | email، password، firstName، lastName، roleId |
| `CreateModifierGroupRequest` | name، selectionType، minSelection، maxSelection، isRequired |

| Response | فیلدهای کلیدی |
|---|---|
| `OrderResponse` | id، orderNumber، channel، status، tableNumber، items[]، invoice؟ |
| `InvoiceResponse` | id، invoiceNumber، channel، status (1=Draft/2=Finalized/3=Cancelled)، paymentMethod؟، subtotal، totalAmount، items[] |
| `MenuItemResponse` | id، groupId، name، price، imageUrl، isAvailable، modifierGroups؟ |
| `UserResponse` | id، email، firstName، lastName، roleId، roleName، permissions[] |
| `SalesReportResponse` | fromUtc، toUtc، invoiceCount، grossSales، netSales، byChannel[]، byPaymentMethod[]، topItems[] |

---

## ۹. لیست کامل enum ها

| Enum | مقادیر |
|---|---|
| OrderStatus | Draft=0, PendingApproval=1, Registered=2, Invoiced=3, Paid=4, Cancelled=5, Rejected=6 |
| InvoiceStatus | Draft=1, Finalized=2, Cancelled=3 |
| PaymentMethod | Cash=1, Card=2, Online=3 |
| PaymentStatus | Draft=0, PendingPayment=1, Paid=2, Cancelled=3 |
| SalesChannel | Online=1, InPerson=2 |
| OrderChannel | Pos=0, Web=1 |
| ReceiptType | Kitchen=1, Customer=2 |
| PosPaymentState | None=0, Pending=1, Succeeded=2, Failed=3, Cancelled=4, TimedOut=5, Unknown=6 |
| StockMovementType | OpeningBalance, InvoiceConsumption, ManualAdjustment |
