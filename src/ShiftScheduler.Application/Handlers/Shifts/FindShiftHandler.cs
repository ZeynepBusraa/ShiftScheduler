using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Mappers;
using ShiftScheduler.Application.Repositories;

namespace ShiftScheduler.Application.Handlers.Shifts;

public class FindShiftHandler(IShiftRepository repository, ShiftDtoMapper mapper)
{
    private readonly IShiftRepository _repository = repository;
    private readonly ShiftDtoMapper _mapper = mapper;

    public async Task<ResponsePayload<ShiftDto>> HandleAsync(int id)
    {
        var shift = await _repository.FindByIdAsync(id);
        
        if (shift == null)
        {
            return new ResponsePayload<ShiftDto>
            {
                Success = false,
                Code = "NOT_FOUND",
                Message = "Kayıt bulunamadı"
            };
        }

        return Response.Ok(_mapper.Map(shift));
    }
}
