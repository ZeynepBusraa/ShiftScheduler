using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Handlers.ShiftRequests;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Api.Controllers;

/// <summary>
/// FR-03.3 → FR-03.5: Nöbet değişim talebi yönetimi.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftRequestsController(
    CreateShiftRequestHandler createHandler,
    ListShiftRequestsHandler listHandler,
    RespondToShiftRequestHandler respondHandler,
    ApproveShiftRequestHandler approveHandler) : ControllerBase
{
    private readonly CreateShiftRequestHandler _createHandler = createHandler;
    private readonly ListShiftRequestsHandler _listHandler = listHandler;
    private readonly RespondToShiftRequestHandler _respondHandler = respondHandler;
    private readonly ApproveShiftRequestHandler _approveHandler = approveHandler;

    /// <summary>FR-03.3: Nöbet değişim talebi oluştur (Asistan/Uzman).</summary>
    [HttpPost]
    [Authorize(Roles = "Uzman, Asistan")]
    public async Task<ResponsePayload<ShiftRequestDto>> Create([FromBody] CreateShiftRequestDto dto)
    {
        var userId = GetCurrentUserId();
        return await _createHandler.HandleAsync(userId, dto);
    }

    /// <summary>Talep listesini döndürür — role göre otomatik filtreleme.</summary>
    [HttpGet]
    public async Task<ResponsePayload<List<ShiftRequestDto>>> List()
    {
        var userId   = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        return await _listHandler.HandleAsync(userId, userRole);
    }

    /// <summary>FR-03.4: Asistanın kendisine gelen talebi kabul veya reddetmesi.</summary>
    [HttpPut("{id}/respond")]
    [Authorize(Roles = "Uzman, Asistan")]
    public async Task<ResponsePayload<ShiftRequestDto>> Respond(
        [FromRoute] int id,
        [FromBody] RespondToShiftRequestDto dto)
    {
        var userId = GetCurrentUserId();
        return await _respondHandler.HandleAsync(id, userId, dto.Accept);
    }

    /// <summary>FR-03.5: Başhekim son onayı verir; onaylanırsa nöbetler takas edilir.</summary>
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Bashekim, Admin")]
    public async Task<ResponsePayload<ShiftRequestDto>> Approve(
        [FromRoute] int id,
        [FromBody] bool approve)
    {
        return await _approveHandler.HandleAsync(id, approve);
    }

    // ─── Yardımcılar ──────────────────────────────────────────────────────────

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    private Role GetCurrentUserRole()
    {
        var claim = User.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<Role>(claim, out var role) ? role : Role.Asistan;
    }
}
