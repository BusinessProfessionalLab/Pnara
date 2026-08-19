namespace Domain.Constants;

public sealed record PermissionSeed(string Name, string Group, string Description);

public static class SystemPermissions
{
    public const string OrdersCreate = "orders.create";
    public const string OrdersView = "orders.view";
    public const string OrdersApprove = "orders.approve";
    public const string InvoicesIssue = "invoices.issue";
    public const string InvoicesPay = "invoices.pay";
    public const string InvoicesCancel = "invoices.cancel";
    public const string InvoicesView = "invoices.view";
    public const string DashboardView = "dashboard.view";
    public const string SettingsManage = "settings.manage";
    public const string Manage = "manage";

    public static readonly PermissionSeed[] Catalog =
    [
        new(OrdersCreate, "سفارشات", "ثبت سفارش"),
        new(OrdersView, "سفارشات", "مشاهده صف سفارشات"),
        new(OrdersApprove, "سفارشات", "تأیید یا رد سفارشات خارجی"),
        new(InvoicesIssue, "فاکتورها", "صدور فاکتور"),
        new(InvoicesPay, "فاکتورها", "ثبت پرداخت فاکتور"),
        new(InvoicesCancel, "فاکتورها", "ابطال فاکتور"),
        new(InvoicesView, "فاکتورها", "مشاهده فاکتورها"),
        new(DashboardView, "داشبورد", "داشبورد مدیریت"),
        new(SettingsManage, "تنظیمات", "تنظیمات سامانه"),
        new(Manage, "مدیریت", "مدیریت منو (افزودن و اصلاح)")
    ];
}
