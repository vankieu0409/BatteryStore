using BatteryShop.Logging;
using IdentityService.Application.Commands;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace IdentityService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IMediator mediator, IUserService userService, ILogger<AccountController> logger)
    {
        _mediator = mediator;
        _userService = userService;
        _logger = logger;
    }    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        // Sử dụng LogHelper để theo dõi performance và thêm context
        return await LogHelper.LogPerformanceAsync<AccountController, IActionResult>(_logger, "Register User", async () =>
        {
            LogHelper.WithContext("Username", command.Username, () => 
            {
                _logger.LogInformation("Đang đăng ký người dùng mới");
            });
            
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _logger.LogInformation("Đăng ký người dùng thành công");
                return Ok(result);
            }

            _logger.LogWarning("Đăng ký người dùng thất bại: {ErrorMessage}", result.ErrorMessage);
            return BadRequest(result);
        });
    }
}
