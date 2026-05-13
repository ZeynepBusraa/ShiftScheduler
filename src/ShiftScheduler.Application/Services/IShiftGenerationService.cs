// src/ShiftScheduler.Application/Services/IShiftGenerationService.cs
using System.Collections.Generic;
using ShiftScheduler.Domain.Entities;

namespace ShiftScheduler.Application.Services
{
    public interface IShiftGenerationService
    {
        List<Shift> GenerateMonthlyShifts(int year, int month, List<User> doctorsInDepartment);
    }
}