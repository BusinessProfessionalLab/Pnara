using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record RejectOrderRequest([Required] string Reason);
