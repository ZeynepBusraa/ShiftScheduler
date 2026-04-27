using System.Collections.Generic;
using System.Linq;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Domain.Entities;

namespace ShiftScheduler.Application.Mappers;

public class ShiftRequestDtoMapper
{
    public ShiftRequestDto Map(ShiftRequest entity)
    {
        return new ShiftRequestDto
        {
            Id             = entity.Id,
            RequesterId    = entity.RequesterId,
            TargetDoctorId = entity.TargetDoctorId,
            ShiftId        = entity.ShiftId,
            Status         = (int)entity.Status
        };
    }

    public List<ShiftRequestDto> MapList(IEnumerable<ShiftRequest> entities)
    {
        return entities.Select(Map).ToList();
    }

    public ShiftRequest ConvertToEntity(ShiftRequestDto dto)
    {
        return new ShiftRequest
        {
            Id             = dto.Id,
            RequesterId    = dto.RequesterId,
            TargetDoctorId = dto.TargetDoctorId,
            ShiftId        = dto.ShiftId,
            Status         = (ShiftScheduler.Domain.Enums.RequestStatus)dto.Status
        };
    }
}

