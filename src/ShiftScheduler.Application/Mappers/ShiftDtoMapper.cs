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
        // Onay durumunu artık doğrudan nöbetten değil, bağlı olduğu Nöbet Listesinden okuyoruz.
        bool isApproved = entity.ShiftList != null && entity.ShiftList.Status == ApprovalStatus.Onaylandi;

        return new ShiftDto(
            entity.Id,
            entity.UserId,
            entity.Date,
            (int)entity.Type,
            isApproved
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
            Type = (ShiftType)dto.ShiftType
            // IsApproved özelliği tablodan kalktığı için artık burada set etmiyoruz.
        };
    }
}