using Application.DTOs;
using Application.Mappers;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

public sealed class PosTerminalManagementService(IPosTerminalRepository repository)
{
    public async Task<IReadOnlyList<PosTerminalResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken)).Select(x => x.ToResponse()).ToList();

    public async Task<PosTerminalResponse> CreateAsync(
        CreatePosTerminalRequest request,
        CancellationToken cancellationToken = default)
    {
        var terminal = PosTerminalDefinition.Create(
            request.Name,
            request.Provider,
            request.ConnectionType,
            request.Host,
            request.Port,
            request.SerialPortName,
            request.BaudRate,
            request.TimeoutSeconds,
            request.IsActive);
        await repository.AddAsync(terminal, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return terminal.ToResponse();
    }

    public async Task<PosTerminalResponse> UpdateAsync(
        Guid id,
        CreatePosTerminalRequest request,
        CancellationToken cancellationToken = default)
    {
        var terminal = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new Application.Exceptions.NotFoundException($"POS terminal with id '{id}' was not found.");
        terminal.Update(
            request.Name,
            request.Provider,
            request.ConnectionType,
            request.Host,
            request.Port,
            request.SerialPortName,
            request.BaudRate,
            request.TimeoutSeconds,
            request.IsActive);
        await repository.SaveChangesAsync(cancellationToken);
        return terminal.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var terminal = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new Application.Exceptions.NotFoundException($"POS terminal with id '{id}' was not found.");
        repository.Remove(terminal);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
