using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Handlers.ShiftLists;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Api.Controllers;

/// <summary>
/// Nöbet listesi yönetimi:
/// - Asistan ve Uzman listelerini ayrı ayrı yönetir.
/// - En kıdemli Asistan → Asistan listesini hazırlar ve başhekime sunar.
/// - En kıdemli Uzman   → Uzman listesini hazırlar ve başhekime sunar.
/// - Başhekim           → Her iki tür listeyi de onaylar veya reddeder.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftListsController(
    ListShiftListsHandler listHandler,
    SubmitShiftListHandler submitHandler,
    ApproveShiftListHandler approveHandler) : ControllerBase
{
    private readonly ListShiftListsHandler _listHandler = listHandler;
    private readonly SubmitShiftListHandler _submitHandler = submitHandler;
    private readonly ApproveShiftListHandler _approveHandler = approveHandler;

    /// <summary>
    /// Rol bazlı nöbet listelerini getirir.
    /// - Başhekim: Tüm bölümlerin tüm listelerini görür.
    /// - Asistan/Uzman: Yalnızca kendi bölümlerinin listelerini görür.
    /// </summary>
    [HttpGet]
    public async Task<ResponsePayload<List<ShiftListDto>>> List()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var deptClaim = User.FindFirst("DepartmentId")?.Value;

        Enum.TryParse<Role>(roleClaim, out var userRole);
        int? departmentId = string.IsNullOrEmpty(deptClaim) ? null : int.Parse(deptClaim);

        return await _listHandler.HandleAsync(userRole, departmentId);
    }

    /// <summary>
    /// Nöbet listesini başhekime onaya sunar.
    /// Sadece listeyi hazırlayan en kıdemli asistan/uzman çağırabilir.
    /// </summary>
    [HttpPut("{id}/submit")]
    [Authorize(Roles = "Asistan, Uzman")]
    public async Task<ResponsePayload<ShiftListDto>> Submit([FromRoute] int id)
    {
        var userId = GetCurrentUserId();
        return await _submitHandler.HandleAsync(id, userId);
    }

    /// <summary>
    /// Başhekim nöbet listesini onaylar veya reddeder.
    /// Reddedilirse liste Taslak'a döner; hazırlayan kişi yeniden düzenleyip sunabilir.
    /// </summary>
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Bashekim")]
    public async Task<ResponsePayload<ShiftListDto>> Approve([FromRoute] int id, [FromBody] bool approve)
    {
        return await _approveHandler.HandleAsync(id, approve);
    }

    // ─── Yardımcılar ──────────────────────────────────────────────────────────

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
