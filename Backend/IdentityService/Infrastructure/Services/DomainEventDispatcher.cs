using BatteryShop.Logging;
using IdentityService.Domain.Common;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Services;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger;
    
    public DomainEventDispatcher(ILogger<DomainEventDispatcher> logger)
    {
        _logger = logger;
    }
      public Task DispatchAsync(IDomainEvent domainEvent)
    {
        return LogHelper.LogPerformanceAsync<DomainEventDispatcher, Task>(_logger, $"Dispatching {domainEvent.GetType().Name}", async () =>
        {
            // Ghi log event để theo dõi với context
            LogHelper.WithContext("DomainEvent", new { 
                Type = domainEvent.GetType().Name, 
                OccurredOn = domainEvent.OccurredOn 
            }, () => {
                _logger.LogInformation("Xử lý Domain Event");
            });
            
            // TODO: Dispatch event to handlers using MediatR or custom event bus
            // Ví dụ: await _mediator.Publish(domainEvent);
            
            return Task.CompletedTask;
        });
    }
}

public interface IDomainEventDispatcher
{
   Task DispatchAsync(IDomainEvent domainEvent);
}
