using System.Collections.Generic;
using System.Linq;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Domain.Entities;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Mappers;

public class ShiftDtoMapper
{
    public ShiftDto Map(Shift entity)
    {
        return new ShiftDto(
            entity.Id,
            entity.UserId,
            entity.Date,
            (int)entity.Type,
            entity.IsApproved
        );
    }

    public List<ShiftDto> MapList(IEnumerable<Shift> entities)
    {
        return entities.Select(Map).ToList();
    }

    public Shift ConvertToEntity(ShiftDto dto)
    {
        return new Shift
        {
            Id = dto.Id,
            UserId = dto.UserId,
            Date = dto.Date,
            Type = (ShiftType)dto.ShiftType,
            IsApproved = dto.IsApproved
        };
    }
}
