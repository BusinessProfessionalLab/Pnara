using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class PosTerminalMapper
{
    public static PosTerminalResponse ToResponse(this PosTerminalDefinition terminal) =>
        new(
            terminal.Id,
            terminal.Name,
            terminal.Provider,
            terminal.ConnectionType,
            terminal.Host,
            terminal.Port,
            terminal.SerialPortName,
            terminal.BaudRate,
            terminal.TimeoutSeconds,
            terminal.IsActive,
            terminal.UpdatedAtUtc);
}
