using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Mappers;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.Shifts;

/// <summary>
/// Nöbet listesini getirir. Rol bazlı görünüm:
/// - Başhekim: Tüm bölümlerin tüm nöbetleri
/// - Asistan: Kendi bölümünün asistan nöbetleri + aynı bölümün uzman nöbetleri (çapraz görünüm)
/// - Uzman: Kendi bölümünün uzman nöbetleri + aynı bölümün asistan nöbetleri (çapraz görünüm)
/// </summary>
public class ListShiftsHandler(IShiftRepository repository, ShiftDtoMapper mapper)
{
    private readonly IShiftRepository _repository = repository;
    private readonly ShiftDtoMapper _mapper = mapper;

    public async Task<ResponsePayload<List<ShiftDto>>> HandleAsync(Role userRole, int? departmentId)
    {
        List<ShiftScheduler.Domain.Entities.Shift> shifts;

        if (userRole == Role.Bashekim)
        {
            // Başhekim tüm bölümlerin tüm nöbetlerini görebilir
            shifts = await _repository.ListAllAsync();
        }
        else
        {
            if (!departmentId.HasValue)
                return Response.Ok(new List<ShiftDto>());

            // Asistan ve Uzman: kendi bölümünün TÜM nöbetlerini görür (hem asistan hem uzman)
            // SEC-02: Sadece kendi bölümü — diğer bölümler görünmez
            shifts = await _repository.ListByDepartmentAsync(departmentId.Value);
        }

        return Response.Ok(_mapper.MapList(shifts));
    }
}
