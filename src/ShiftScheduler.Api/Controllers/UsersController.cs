using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Services;

namespace ShiftScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpPost]
    [Authorize(Roles = "Bashekim, Admin")] // Sadece Başhekim ve Admin yeni kullanıcı ekleyebilir
    public async Task<ResponsePayload<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        return await _userService.CreateAsync(request);
    }

    [HttpGet]
    [Authorize(Roles = "Bashekim, Admin")] // Listelemeyi sadece yetkililer yapabilir
    public async Task<ResponsePayload<List<UserDto>>> List()
    {
        return await _userService.ListAsync();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Bashekim, Admin")]
    public async Task<ResponsePayload<bool>> Delete(int id)
    {
        return await _userService.DeleteAsync(id);
    }
}
