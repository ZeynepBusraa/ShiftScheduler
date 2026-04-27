using System.Collections.Generic;
using System.Linq;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Domain.Entities;

namespace ShiftScheduler.Application.Mappers;

public class ShiftRequestDtoMapper
{
    public ShiftRequestDto Map(ShiftRequest entity)
    {
        return new ShiftRequestDto(
            entity.Id,
            entity.RequesterId,
            entity.TargetDoctorId,
            entity.ShiftId,
            entity.Status
        );
    }

    public List<ShiftRequestDto> MapList(IEnumerable<ShiftRequest> entities)
    {
        return entities.Select(Map).ToList();
    }

    public ShiftRequest ConvertToEntity(ShiftRequestDto dto)
    {
        return new ShiftRequest
        {
            Id = dto.Id,
            RequesterId = dto.RequesterId,
            TargetDoctorId = dto.TargetDoctorId,
            ShiftId = dto.ShiftId,
            Status = dto.Status
        };
    }
}
