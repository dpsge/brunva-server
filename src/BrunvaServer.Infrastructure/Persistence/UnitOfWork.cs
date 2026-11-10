using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;

namespace BrunvaServer.Infrastructure.Persistence;

public sealed class UnitOfWork(BrunvaDbContext db) : IUnitOfWork, IAsyncDisposable
{
    private IDbContextTransaction? _transaction;

    public async Task BeginAsync(CancellationToken cancellationToken)
    {
        _transaction ??= await db.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            if (_transaction is null)
                return;

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ConflictException.Concurrency();
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw ConflictException.Duplicate();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await _transaction.DisposeAsync();
    }
}
