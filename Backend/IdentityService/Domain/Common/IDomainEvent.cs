namespace IdentityService.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
