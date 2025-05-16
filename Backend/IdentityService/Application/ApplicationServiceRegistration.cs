using BatteryShop.Logging;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using IdentityService.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;

namespace IdentityService.Application;

public static class ApplicationServiceRegistration
{    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Đăng ký các services
        services.AddScoped<IUserService, UserService>();
       // services.AddScoped<IIdentityService, Services.IdentityService>();
        
        // Đăng ký MediatR
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            
        // Đăng ký LoggingBehavior từ thư viện BatteryShop.Logging
        services.AddLoggingBehavior();
        
        // Đăng ký FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Đăng ký Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        return services;
    }
}
