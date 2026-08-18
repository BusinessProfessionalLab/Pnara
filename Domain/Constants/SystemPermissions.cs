namespace Domain.Constants;

public sealed record PermissionSeed(string Name, string Group, string Description);

public static class SystemPermissions
{
    public const string OrdersCreate = "orders.create";
    public const string DashboardView = "dashboard.view";
    public const string SettingsManage = "settings.manage";
    public const string Manage = "manage";

    public static readonly PermissionSeed[] Catalog =
    [
        new(OrdersCreate, "سفارشات", "ثبت سفارش"),
        new(DashboardView, "داشبورد", "داشبورد مدیریت"),
        new(SettingsManage, "تنظیمات", "تنظیمات سامانه"),
        new(Manage, "مدیریت", "مدیریت منو (افزودن و اصلاح)")
    ];
}
