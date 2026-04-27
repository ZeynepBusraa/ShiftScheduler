using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Mappers;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.Shifts;

public class ListShiftsHandler(IShiftRepository repository, ShiftDtoMapper mapper)
{
    private readonly IShiftRepository _repository = repository;
    private readonly ShiftDtoMapper _mapper = mapper;

    public async Task<ResponsePayload<List<ShiftDto>>> HandleAsync(Role userRole, int? departmentId)
    {
        List<ShiftScheduler.Domain.Entities.Shift> shifts;

        // Başhekim veya Admin ise tüm listeyi görebilir
        if (userRole == Role.Bashekim || userRole == Role.Admin)
        {
            shifts = await _repository.ListAllAsync();
        }
        else
        {
            // Uzman ve Asistanlar sadece kendi bölümlerinin listesini görebilir
            if (departmentId.HasValue)
            {
                shifts = await _repository.ListByDepartmentAsync(departmentId.Value);
            }
            else
            {
                shifts = new List<ShiftScheduler.Domain.Entities.Shift>();
            }
        }

        return Response.Ok(_mapper.MapList(shifts));
    }
}
