using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Xunit;

namespace Domain.Tests;

public class PosTerminalTests
{
    [Fact]
    public void TcpTerminal_RequiresHostAndPort()
    {
        Assert.Throws<DomainException>(() =>
            PosTerminalDefinition.Create(
                "Cashier",
                "TCP",
                PosTerminalConnectionType.Tcp,
                null,
                null,
                null,
                null));
    }

    [Fact]
    public void SerialTerminal_RequiresPortAndBaudRate()
    {
        Assert.Throws<DomainException>(() =>
            PosTerminalDefinition.Create(
                "Cashier",
                "Serial",
                PosTerminalConnectionType.Serial,
                null,
                null,
                "COM3",
                null));
    }
}
