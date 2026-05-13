using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.ShiftRequests;

/// <summary>
/// Talep listesini döndürür. Rol bazlı filtreleme:
/// - Başhekim → başhekim onayı bekleyen tüm talepler (KidemliOnayladi durumunda)
/// - Uzman/Asistan → kendi oluşturduğu + kendisine gelen talepler
/// </summary>
public class ListShiftRequestsHandler(IShiftRequestRepository requestRepository)
{
    private readonly IShiftRequestRepository _requestRepository = requestRepository;

    public async Task<ResponsePayload<List<ShiftRequestDto>>> HandleAsync(int userId, Role userRole)
    {
        List<ShiftScheduler.Domain.Entities.ShiftRequest> requests;

        if (userRole == Role.Bashekim)
        {
            // Başhekim: kıdemli tarafından onaylanmış, başhekim onayı bekleyenleri listele
            requests = await _requestRepository.ListPendingChiefApprovalAsync();
        }
        else
        {
            // Kendi oluşturduklarını getir
            var sent = await _requestRepository.ListByRequesterAsync(userId);
            // Kendisine gelenleri getir
            var received = await _requestRepository.ListPendingForTargetDoctorAsync(userId);

            // Birleştir, tekrarları kaldır
            var combined = new Dictionary<int, ShiftScheduler.Domain.Entities.ShiftRequest>();
            foreach (var r in sent)    combined[r.Id] = r;
            foreach (var r in received) combined[r.Id] = r;
            requests = new List<ShiftScheduler.Domain.Entities.ShiftRequest>(combined.Values);
        }

        var dtos = requests.ConvertAll(r => new ShiftRequestDto
        {
            Id             = r.Id,
            RequesterId    = r.RequesterId,
            TargetDoctorId = r.TargetDoctorId,
            ShiftId        = r.ShiftId,
            Status         = (int)r.Status
        });

        return Response.Ok(dtos);
    }
}
