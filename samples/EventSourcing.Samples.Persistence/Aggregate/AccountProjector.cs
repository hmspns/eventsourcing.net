using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.ServiceRegistration;
using EventSourcing.Samples.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Samples.Persistence.Aggregate;

public sealed class AccountProjector : 
    IEventConsumer<Guid, AccountCreatedEvent>,
    IEventConsumer<Guid, AccountReplenishedEvent>,
    IEventConsumer<Guid, AccountWithdrawnEvent>,
    IEventConsumer<Guid, AccountClosedEvent>
{
    private readonly ApplicationDbContext _context;

    public AccountProjector(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Consume(IEventEnvelope<Guid, AccountCreatedEvent> envelope)
    {
        _context.Accounts.Add(new AccountDb()
        {
            OwnerName = envelope.Payload.OwnerName,
            Id = envelope.AggregateId,
            ClosingDate = null,
            CreationDate = envelope.Timestamp
        });
        await _context.SaveChangesAsync();
    }

    public async Task Consume(IEventEnvelope<Guid, AccountReplenishedEvent> envelope)
    {
        _context.AccountOperations.Add(new AccountOperationDb()
        {
            Amount = envelope.Payload.Amount,
            Id = envelope.Payload.OperationId,
            AccountId = envelope.AggregateId,
            IsWithdrawn = false,
            OperationDate = envelope.Timestamp
        });
        await _context.SaveChangesAsync();
    }

    public async Task Consume(IEventEnvelope<Guid, AccountWithdrawnEvent> envelope)
    {
        _context.AccountOperations.Add(new AccountOperationDb()
        {
            Amount = envelope.Payload.Amount * -1,
            Id = envelope.Payload.OperationId,
            AccountId = envelope.AggregateId,
            IsWithdrawn = true,
            OperationDate = envelope.Timestamp
        });
        await _context.SaveChangesAsync();
    }

    public async Task Consume(IEventEnvelope<Guid, AccountClosedEvent> envelope)
    {
        AccountDb? account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == envelope.AggregateId);
        if (account != null)
        {
            account.ClosingDate = envelope.Timestamp;
            await _context.SaveChangesAsync();
        }
    }
}