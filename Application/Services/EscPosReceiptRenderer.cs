using System.Globalization;
using System.Text;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

internal static class EscPosReceiptRenderer
{
    private static readonly Encoding ReceiptEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static byte[] Render(
        Invoice invoice,
        CompanyInfo? companyInfo,
        ReceiptTemplate template,
        int paperWidth,
        ReceiptType receiptType)
    {
        var width = paperWidth == 80 ? 48 : 32;
        var lines = new List<string>();
        var commands = new List<byte>();

        commands.AddRange([0x1B, 0x40]);
        commands.AddRange([0x1B, 0x21, (byte)(template.FontSize == 3 ? 0x30 : template.FontSize == 2 ? 0x10 : 0x00)]);

        if (!string.IsNullOrWhiteSpace(template.HeaderText))
            lines.AddRange(Wrap(template.HeaderText, width));

        if (receiptType == ReceiptType.Customer && companyInfo is not null)
        {
            if (template.ShowLogo)
                lines.Add(Center(companyInfo.Name, width));
            else
                lines.Add(companyInfo.Name);
        }

        lines.Add(receiptType == ReceiptType.Kitchen ? "KITCHEN ORDER" : "CUSTOMER RECEIPT");
        lines.Add($"Invoice: {invoice.InvoiceNumber}");
        lines.Add($"Time: {invoice.IssuedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm}");
        if (template.ShowChannel)
            lines.Add($"Channel: {invoice.Channel}");
        lines.Add(new string('-', width));

        foreach (var item in invoice.Items)
        {
            var itemLine = $"{FormatQuantity(item.Quantity)} x {item.ItemName}";
            if (receiptType == ReceiptType.Customer && template.ShowPrices)
                lines.Add(Columns(itemLine, FormatMoney(item.LineTotal), width));
            else
                lines.AddRange(Wrap(itemLine, width));

            foreach (var addon in item.Addons)
            {
                var addonLine = $"  + {FormatQuantity(addon.Quantity)} x {addon.AddonName}";
                if (receiptType == ReceiptType.Customer && template.ShowPrices)
                    lines.Add(Columns(addonLine, FormatMoney(addon.LineTotal), width));
                else
                    lines.AddRange(Wrap(addonLine, width));
            }
        }

        if (receiptType == ReceiptType.Customer)
        {
            lines.Add(new string('-', width));
            if (template.ShowPrices)
                lines.Add(Columns("Subtotal", FormatMoney(invoice.Subtotal), width));
            if (template.ShowDiscount && invoice.DiscountAmount > 0)
                lines.Add(Columns("Discount", FormatMoney(invoice.DiscountAmount), width));
            if (template.ShowTax && invoice.TaxAmount > 0)
                lines.Add(Columns("Tax", FormatMoney(invoice.TaxAmount), width));
            if (template.ShowPrices)
                lines.Add(Columns("TOTAL", FormatMoney(invoice.TotalAmount), width));
            if (template.ShowPaymentMethod && invoice.PaymentMethod.HasValue)
                lines.Add($"Payment: {invoice.PaymentMethod}");
        }

        if (!string.IsNullOrWhiteSpace(template.FooterText))
        {
            lines.Add(new string('-', width));
            lines.AddRange(Wrap(template.FooterText, width));
        }

        foreach (var line in lines)
        {
            commands.AddRange(ReceiptEncoding.GetBytes(line));
            commands.Add(0x0A);
        }

        commands.AddRange([0x1B, 0x21, 0x00]);
        commands.AddRange(ReceiptEncoding.GetBytes("\n\n"));
        commands.AddRange([0x1D, 0x56, 0x01]);
        return commands.ToArray();
    }

    private static string FormatMoney(decimal amount) =>
        amount.ToString("N2", CultureInfo.InvariantCulture);

    private static string FormatQuantity(decimal quantity) =>
        quantity.ToString("0.###", CultureInfo.InvariantCulture);

    private static string Columns(string left, string right, int width)
    {
        if (left.Length + right.Length + 1 > width)
            left = left[..Math.Max(1, width - right.Length - 1)];

        return left.PadRight(width - right.Length) + right;
    }

    private static IEnumerable<string> Wrap(string text, int width)
    {
        foreach (var rawLine in text.Replace("\r", string.Empty).Split('\n'))
        {
            var remaining = rawLine;
            while (remaining.Length > width)
            {
                var splitAt = remaining.LastIndexOf(' ', width - 1);
                if (splitAt <= 0)
                    splitAt = width;

                yield return remaining[..splitAt];
                remaining = remaining[splitAt..].TrimStart();
            }

            yield return remaining;
        }
    }

    private static string Center(string value, int width)
    {
        if (value.Length >= width)
            return value[..width];

        var padding = (width - value.Length) / 2;
        return new string(' ', padding) + value;
    }
}
