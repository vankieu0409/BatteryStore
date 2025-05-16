
using IdentityService.Domain.Common;

namespace IdentityService.Application.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent);
}
