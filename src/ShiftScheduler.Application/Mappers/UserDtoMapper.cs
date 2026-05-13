using System.Collections.Generic;
using System.Linq;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Domain.Entities;

namespace ShiftScheduler.Application.Mappers;

public class UserDtoMapper
{
    public UserDto Map(User entity)
    {
        return new UserDto(
            entity.Id,
            entity.Email,
            entity.Role,
            entity.SeniorityYear,
            entity.DepartmentId
        );
    }

    public List<UserDto> MapList(IEnumerable<User> entities)
    {
        return entities.Select(Map).ToList();
    }

    public User ConvertToEntity(UserDto dto)
    {
        return new User
        {
            Id = dto.Id,
            Email = dto.Email,
            Role = dto.Role,
            SeniorityYear = dto.SeniorityYear,
            DepartmentId = dto.DepartmentId
        };
    }
}