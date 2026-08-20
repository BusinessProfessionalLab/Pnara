using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class PrinterDefinition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public PrinterConnectionType ConnectionType { get; private set; }
    public string Host { get; private set; } = null!;
    public int Port { get; private set; }
    public int PaperWidth { get; private set; }
    public bool IsActive { get; private set; }

    private PrinterDefinition()
    {
    }

    private PrinterDefinition(
        string name,
        PrinterConnectionType connectionType,
        string host,
        int port,
        int paperWidth)
    {
        Id = Guid.NewGuid();
        Name = name;
        ConnectionType = connectionType;
        Host = host;
        Port = port;
        PaperWidth = paperWidth;
        IsActive = true;
    }

    public static PrinterDefinition Create(
        string name,
        PrinterConnectionType connectionType,
        string host,
        int port,
        int paperWidth)
    {
        Validate(name, connectionType, host, port, paperWidth);
        return new PrinterDefinition(
            name.Trim(),
            connectionType,
            host.Trim(),
            port,
            paperWidth);
    }

    public void Update(
        string name,
        PrinterConnectionType connectionType,
        string host,
        int port,
        int paperWidth,
        bool isActive)
    {
        Validate(name, connectionType, host, port, paperWidth);

        Name = name.Trim();
        ConnectionType = connectionType;
        Host = host.Trim();
        Port = port;
        PaperWidth = paperWidth;
        IsActive = isActive;
    }

    private static void Validate(
        string name,
        PrinterConnectionType connectionType,
        string host,
        int port,
        int paperWidth)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Printer name cannot be empty.");

        if (!Enum.IsDefined(connectionType))
            throw new DomainException("Printer connection type is invalid.");

        if (string.IsNullOrWhiteSpace(host))
            throw new DomainException("Printer host cannot be empty.");

        if (port is < 1 or > 65535)
            throw new DomainException("Printer port must be between 1 and 65535.");

        if (paperWidth is not (58 or 80))
            throw new DomainException("Printer paper width must be 58 or 80 millimeters.");
    }
}
