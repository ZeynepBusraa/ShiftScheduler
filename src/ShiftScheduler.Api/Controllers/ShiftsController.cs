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
    [Authorize(Roles = "Bashekim, Admin")]
    public async Task<ResponsePayload<GenerateShiftsResult>> GenerateList([FromBody] GenerateShiftsRequest request)
    {
        // FR-02: Nöbet atama algoritmasını çalıştır
        return await _shiftService.GenerateAsync(request);
    }
}
