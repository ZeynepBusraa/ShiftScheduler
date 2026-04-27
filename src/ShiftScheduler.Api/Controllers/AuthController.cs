using Microsoft.AspNetCore.Mvc;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs.Auth;
using ShiftScheduler.Application.Handlers.Auth;

namespace ShiftScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(LoginUserHandler loginHandler) : ControllerBase
{
    private readonly LoginUserHandler _loginHandler = loginHandler;

    [HttpPost("login")]
    public async Task<ResponsePayload<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        return await _loginHandler.HandleAsync(request);
    }
}
