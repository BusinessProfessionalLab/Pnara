using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record AssignPermissionsRequest([Required] Guid[] PermissionIds);
