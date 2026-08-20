using Domain.Enums;

namespace Application.DTOs;

public record PosTerminalResponse(
    Guid Id,
    string Name,
    string Provider,
    PosTerminalConnectionType ConnectionType,
    string? Host,
    int? Port,
    string? SerialPortName,
    int? BaudRate,
    int TimeoutSeconds,
    bool IsActive,
    DateTime UpdatedAtUtc);

public record CreatePosTerminalRequest(
    string Name,
    string Provider,
    PosTerminalConnectionType ConnectionType,
    string? Host,
    int? Port,
    string? SerialPortName,
    int? BaudRate,
    int TimeoutSeconds = 60,
    bool IsActive = true);
