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

    /// <summary>
    /// FR-02: Nöbet dağıtım algoritmasını çalıştırır.
    /// - En kıdemli Asistan → kendi bölümü için Asistan listesi oluşturur.
    /// - En kıdemli Uzman   → kendi bölümü için Uzman listesi oluşturur.
    /// - Başhekim           → herhangi bir bölüm/liste türü için çalıştırabilir.
    /// Yetki detayı GenerateShiftsHandler içinde kontrol edilir.
    /// </summary>
    [HttpPost("generate")]
    [Authorize(Roles = "Bashekim, Uzman, Asistan")]
    public async Task<ResponsePayload<GenerateShiftsResult>> GenerateList([FromBody] GenerateShiftsRequest request)
    {
        var userId = GetCurrentUserId();
        return await _shiftService.GenerateAsync(request, userId);
    }

    // ─── Yardımcı ─────────────────────────────────────────────────────────────
    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}