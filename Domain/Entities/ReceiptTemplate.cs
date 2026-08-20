using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class ReceiptTemplate
{
    public Guid Id { get; private set; }
    public ReceiptType ReceiptType { get; private set; }
    public string? HeaderText { get; private set; }
    public string? FooterText { get; private set; }
    public bool ShowLogo { get; private set; }
    public bool ShowPrices { get; private set; }
    public bool ShowDiscount { get; private set; }
    public bool ShowTax { get; private set; }
    public bool ShowPaymentMethod { get; private set; }
    public bool ShowChannel { get; private set; }
    public int FontSize { get; private set; }
    public bool IsActive { get; private set; }

    private ReceiptTemplate()
    {
    }

    private ReceiptTemplate(
        ReceiptType receiptType,
        string? headerText,
        string? footerText,
        bool showLogo,
        bool showPrices,
        bool showDiscount,
        bool showTax,
        bool showPaymentMethod,
        bool showChannel,
        int fontSize,
        bool isActive = true)
    {
        Id = Guid.NewGuid();
        ReceiptType = receiptType;
        HeaderText = NormalizeText(headerText);
        FooterText = NormalizeText(footerText);
        ShowLogo = showLogo;
        ShowPrices = showPrices;
        ShowDiscount = showDiscount;
        ShowTax = showTax;
        ShowPaymentMethod = showPaymentMethod;
        ShowChannel = showChannel;
        FontSize = fontSize;
        IsActive = isActive;
    }

    public static ReceiptTemplate Create(
        ReceiptType receiptType,
        string? headerText,
        string? footerText,
        bool showLogo,
        bool showPrices,
        bool showDiscount,
        bool showTax,
        bool showPaymentMethod,
        bool showChannel,
        int fontSize,
        bool isActive = true)
    {
        Validate(receiptType, fontSize);
        return new ReceiptTemplate(
            receiptType,
            headerText,
            footerText,
            showLogo,
            showPrices,
            showDiscount,
            showTax,
            showPaymentMethod,
            showChannel,
            fontSize,
            isActive);
    }

    public void Update(
        string? headerText,
        string? footerText,
        bool showLogo,
        bool showPrices,
        bool showDiscount,
        bool showTax,
        bool showPaymentMethod,
        bool showChannel,
        int fontSize,
        bool isActive)
    {
        Validate(ReceiptType, fontSize);
        HeaderText = NormalizeText(headerText);
        FooterText = NormalizeText(footerText);
        ShowLogo = showLogo;
        ShowPrices = showPrices;
        ShowDiscount = showDiscount;
        ShowTax = showTax;
        ShowPaymentMethod = showPaymentMethod;
        ShowChannel = showChannel;
        FontSize = fontSize;
        IsActive = isActive;
    }

    private static void Validate(ReceiptType receiptType, int fontSize)
    {
        if (!Enum.IsDefined(receiptType))
            throw new DomainException("Receipt type is invalid.");

        if (fontSize is < 1 or > 3)
            throw new DomainException("Receipt font size must be between 1 and 3.");
    }

    private static string? NormalizeText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
