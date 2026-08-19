using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record IssueInvoiceRequest(
    [Range(0, double.MaxValue)] decimal Discount);
