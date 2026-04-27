using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Mappers;
using ShiftScheduler.Application.Repositories;

namespace ShiftScheduler.Application.Handlers.Shifts;

public class SaveShiftHandler(IShiftRepository repository, ShiftDtoMapper mapper)
{
    private readonly IShiftRepository _repository = repository;
    private readonly ShiftDtoMapper _mapper = mapper;

    public async Task<ResponsePayload<ShiftDto>> HandleAsync(ShiftDto dto)
    {
        // Şimdilik karmaşık iş kuralları yok, sadece basit CRUD kayıt işlemi
        var entity = _mapper.ConvertToEntity(dto);
        
        var savedEntity = await _repository.SaveAsync(entity);
        
        return Response.SaveSuccess(_mapper.Map(savedEntity));
    }
}
