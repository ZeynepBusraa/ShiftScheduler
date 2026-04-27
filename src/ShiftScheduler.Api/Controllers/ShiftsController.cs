using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Services;

using Microsoft.AspNetCore.Authorization;
using ShiftScheduler.Domain.Enums;
using System.Security.Claims;
using System;

namespace ShiftScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftsController(IShiftService shiftService) : ControllerBase
{
    private readonly IShiftService _shiftService = shiftService;

    [HttpGet("{id}")]
    public async Task<ResponsePayload<ShiftDto>> Find([FromRoute] int id)
    {
        return await _shiftService.FindAsync(id);
    }

    [HttpPost("save")]
    public async Task<ResponsePayload<ShiftDto>> Save([FromBody] ShiftDto dto)
    {
        return await _shiftService.SaveAsync(dto);
    }

    [HttpGet("list")]
    public async Task<ResponsePayload<List<ShiftDto>>> List()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var deptClaim = User.FindFirst("DepartmentId")?.Value;

        Enum.TryParse<Role>(roleClaim, out var userRole);
        int? departmentId = string.IsNullOrEmpty(deptClaim) ? null : int.Parse(deptClaim);

        return await _shiftService.ListAsync(userRole, departmentId);
    }

    [HttpPost("generate")]
    [Authorize(Roles = "Uzman, Asistan")]
    public async Task<ResponsePayload<bool>> GenerateList()
    {
        // TODO: En kıdemsiz uzman veya en kıdemli asistan kontrolü burada veya Handler içerisinde yapılacak.
        return await Task.FromResult(Application.Common.Response.Ok(true));
    }
}
