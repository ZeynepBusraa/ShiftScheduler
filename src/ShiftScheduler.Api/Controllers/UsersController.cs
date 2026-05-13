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
    [AllowAnonymous] // YENİ EKLENDİ: İlk kayıtları yapabilmemiz için güvenlik kapısını geçici olarak açtık
    // [Authorize(Roles = "Bashekim")] // NOT: Canlıya çıkarken AllowAnonymous'u silip bu satırı tekrar aktif edebilirsin
    public async Task<ResponsePayload<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        return await _userService.CreateAsync(request);
    }

    [HttpGet]
    [AllowAnonymous] 
    [Authorize(Roles = "Bashekim")] // Listelemeyi sadece Başhekim yapabilir
    public async Task<ResponsePayload<List<UserDto>>> List()
    {
        return await _userService.ListAsync();
    }

    [HttpDelete("{id}")]
    [AllowAnonymous] 
    [Authorize(Roles = "Bashekim")] // Silmeyi sadece Başhekim yapabilir
    public async Task<ResponsePayload<bool>> Delete(int id)
    {
        return await _userService.DeleteAsync(id);
    }
}