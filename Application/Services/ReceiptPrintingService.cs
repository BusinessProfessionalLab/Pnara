using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappers;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using System.Net.Sockets;

namespace Application.Services;

public class ReceiptPrintingService(
    IPrintingRepository printingRepository,
    IInvoiceRepository invoiceRepository,
    ICompanyInfoRepository companyInfoRepository,
    IReceiptPrinterClient printerClient) : IReceiptPrintingService
{
    public async Task<PrinterResponse> CreatePrinterAsync(
        CreatePrinterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await printingRepository.PrinterNameExistsAsync(
                request.Name,
                cancellationToken: cancellationToken))
        {
            throw new Domain.Exceptions.DomainException(
                "A printer with the same name already exists.");
        }

        var printer = PrinterDefinition.Create(
            request.Name,
            request.ConnectionType,
            request.Host,
            request.Port,
            request.PaperWidth);
        await printingRepository.AddPrinterAsync(printer, cancellationToken);
        await printingRepository.SaveChangesAsync(cancellationToken);
        return printer.ToResponse();
    }

    public async Task<PrinterResponse> UpdatePrinterAsync(
        Guid id,
        UpdatePrinterRequest request,
        CancellationToken cancellationToken = default)
    {
        var printer = await printingRepository.GetPrinterByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Printer with id '{id}' was not found.");

        if (await printingRepository.PrinterNameExistsAsync(
                request.Name,
                id,
                cancellationToken))
        {
            throw new Domain.Exceptions.DomainException(
                "A printer with the same name already exists.");
        }

        printer.Update(
            request.Name,
            request.ConnectionType,
            request.Host,
            request.Port,
            request.PaperWidth,
            request.IsActive);
        await printingRepository.SaveChangesAsync(cancellationToken);
        return printer.ToResponse();
    }

    public async Task<IReadOnlyList<PrinterResponse>> GetPrintersAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        (await printingRepository.GetPrintersAsync(
            includeInactive,
            cancellationToken))
        .Select(printer => printer.ToResponse())
        .ToList();

    public async Task<ReceiptTemplateResponse> UpsertTemplateAsync(
        ReceiptType receiptType,
        UpsertReceiptTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var template = await printingRepository.GetTemplateAsync(
            receiptType,
            includeInactive: true,
            cancellationToken);

        if (template is null)
        {
            template = ReceiptTemplate.Create(
                receiptType,
                request.HeaderText,
                request.FooterText,
                request.ShowLogo,
                request.ShowPrices,
                request.ShowDiscount,
                request.ShowTax,
                request.ShowPaymentMethod,
                request.ShowChannel,
                request.FontSize,
                request.IsActive);
            await printingRepository.AddTemplateAsync(template, cancellationToken);
        }
        else
        {
            template.Update(
                request.HeaderText,
                request.FooterText,
                request.ShowLogo,
                request.ShowPrices,
                request.ShowDiscount,
                request.ShowTax,
                request.ShowPaymentMethod,
                request.ShowChannel,
                request.FontSize,
                request.IsActive);
        }

        await printingRepository.SaveChangesAsync(cancellationToken);
        return template.ToResponse();
    }

    public async Task<IReadOnlyList<ReceiptTemplateResponse>> GetTemplatesAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        (await printingRepository.GetTemplatesAsync(
            includeInactive,
            cancellationToken))
        .Select(template => template.ToResponse())
        .ToList();

    public async Task<ReceiptPrinterMappingResponse> AssignPrinterAsync(
        ReceiptType receiptType,
        AssignReceiptPrinterRequest request,
        CancellationToken cancellationToken = default)
    {
        var printer = await printingRepository.GetPrinterByIdAsync(
            request.PrinterDefinitionId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Printer with id '{request.PrinterDefinitionId}' was not found.");

        if (!printer.IsActive)
            throw new Domain.Exceptions.DomainException(
                "An inactive printer cannot be assigned to a receipt.");

        var mapping = await printingRepository.GetMappingAsync(
            receiptType,
            cancellationToken);
        if (mapping is null)
        {
            mapping = ReceiptPrinterMapping.Create(
                receiptType,
                printer.Id);
            await printingRepository.AddMappingAsync(mapping, cancellationToken);
        }
        else
        {
            mapping.AssignPrinter(printer.Id);
        }

        await printingRepository.SaveChangesAsync(cancellationToken);
        return new ReceiptPrinterMappingResponse(
            receiptType,
            printer.Id,
            printer.Name);
    }

    public async Task<IReadOnlyList<ReceiptPrinterMappingResponse>> GetMappingsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new List<ReceiptPrinterMappingResponse>();
        foreach (var receiptType in Enum.GetValues<ReceiptType>())
        {
            var mapping = await printingRepository.GetMappingAsync(
                receiptType,
                cancellationToken);
            if (mapping?.PrinterDefinition is not null)
            {
                result.Add(new ReceiptPrinterMappingResponse(
                    receiptType,
                    mapping.PrinterDefinitionId,
                    mapping.PrinterDefinition.Name));
            }
        }

        return result;
    }

    public async Task<PrintReceiptResponse> PrintAsync(
        Guid invoiceId,
        ReceiptType receiptType,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(receiptType))
            throw new Domain.Exceptions.DomainException("Receipt type is invalid.");

        var invoice = await invoiceRepository.GetByIdAsync(
            invoiceId,
            cancellationToken)
            ?? throw new NotFoundException($"Invoice with id '{invoiceId}' was not found.");

        if (invoice.Status != InvoiceStatus.Finalized)
            throw new Domain.Exceptions.DomainException(
                "Only finalized invoices can be printed.");

        var attemptedAtUtc = DateTime.UtcNow;
        var template = await printingRepository.GetTemplateAsync(
            receiptType,
            includeInactive: true,
            cancellationToken);
        if (template is null || !template.IsActive)
        {
            return new PrintReceiptResponse(
                invoiceId,
                receiptType,
                Printed: false,
                Skipped: true,
                "No active receipt template is configured.",
                PrinterName: null,
                attemptedAtUtc);
        }

        var mapping = await printingRepository.GetMappingAsync(
            receiptType,
            cancellationToken);
        if (mapping?.PrinterDefinition is null || !mapping.PrinterDefinition.IsActive)
        {
            return new PrintReceiptResponse(
                invoiceId,
                receiptType,
                Printed: false,
                Skipped: true,
                "No active printer is mapped to this receipt type.",
                PrinterName: mapping?.PrinterDefinition?.Name,
                attemptedAtUtc);
        }

        var companyInfo = await companyInfoRepository.GetAsync(cancellationToken);
        var payload = EscPosReceiptRenderer.Render(
            invoice,
            companyInfo,
            template,
            mapping.PrinterDefinition.PaperWidth,
            receiptType);

        try
        {
            await printerClient.SendAsync(
                mapping.PrinterDefinition,
                payload,
                cancellationToken);
        }
        catch (Exception exception) when (
            exception is IOException or SocketException or TimeoutException)
        {
            throw new PrintingException(
                $"Could not print the {receiptType} receipt on printer '{mapping.PrinterDefinition.Name}'.",
                exception);
        }

        return new PrintReceiptResponse(
            invoiceId,
            receiptType,
            Printed: true,
            Skipped: false,
            "Receipt sent to the printer.",
            mapping.PrinterDefinition.Name,
            attemptedAtUtc);
    }
}
