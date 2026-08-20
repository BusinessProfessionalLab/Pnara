using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class PosTerminalDefinition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Provider { get; private set; } = null!;
    public PosTerminalConnectionType ConnectionType { get; private set; }
    public string? Host { get; private set; }
    public int? Port { get; private set; }
    public string? SerialPortName { get; private set; }
    public int? BaudRate { get; private set; }
    public int TimeoutSeconds { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private PosTerminalDefinition() { }

    private PosTerminalDefinition(
        string name,
        string provider,
        PosTerminalConnectionType connectionType,
        string? host,
        int? port,
        string? serialPortName,
        int? baudRate,
        int timeoutSeconds,
        bool isActive)
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
        Update(name, provider, connectionType, host, port, serialPortName, baudRate, timeoutSeconds, isActive);
    }

    public static PosTerminalDefinition Create(
        string name,
        string provider,
        PosTerminalConnectionType connectionType,
        string? host,
        int? port,
        string? serialPortName,
        int? baudRate,
        int timeoutSeconds = 60,
        bool isActive = true) =>
        new(name, provider, connectionType, host, port, serialPortName, baudRate, timeoutSeconds, isActive);

    public void Update(
        string name,
        string provider,
        PosTerminalConnectionType connectionType,
        string? host,
        int? port,
        string? serialPortName,
        int? baudRate,
        int timeoutSeconds,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            throw new DomainException("Terminal name is required and cannot exceed 100 characters.");
        if (string.IsNullOrWhiteSpace(provider) || provider.Trim().Length > 100)
            throw new DomainException("Terminal provider is required and cannot exceed 100 characters.");
        if (!Enum.IsDefined(connectionType))
            throw new DomainException("Terminal connection type is invalid.");
        if (timeoutSeconds is < 1 or > 300)
            throw new DomainException("Terminal timeout must be between 1 and 300 seconds.");

        if (connectionType == PosTerminalConnectionType.Tcp &&
            (string.IsNullOrWhiteSpace(host) || port is < 1 or > 65535))
            throw new DomainException("TCP terminals require a valid host and port.");
        if (connectionType == PosTerminalConnectionType.Serial &&
            (string.IsNullOrWhiteSpace(serialPortName) || !baudRate.HasValue || baudRate is < 1200 or > 1000000))
            throw new DomainException("Serial terminals require a port name and baud rate.");

        Name = name.Trim();
        Provider = provider.Trim();
        ConnectionType = connectionType;
        Host = host?.Trim();
        Port = port;
        SerialPortName = serialPortName?.Trim();
        BaudRate = baudRate;
        TimeoutSeconds = timeoutSeconds;
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
