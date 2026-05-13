// src/ShiftScheduler.Application/Services/ShiftGenerationService.cs
// NOT: Bu servis artık kullanılmamaktadır. Nöbet üretim mantığı tamamen
// GenerateShiftsHandler.cs'e (Handlers/Shifts/) taşınmıştır.
// Kütüphane derlenebilir kalması için arayüz implementasyonu korunmuştur.
using System;
using System.Collections.Generic;
using System.Linq;
using ShiftScheduler.Domain.Entities;

namespace ShiftScheduler.Application.Services
{
    public class ShiftGenerationService : IShiftGenerationService
    {
        private bool CanAssignShift(int doctorId, DateTime date, List<Shift> currentShifts)
        {
            // FR-02.2: Üst üste iki gün nöbet atanamaz
            bool hasShiftYesterday = currentShifts.Any(s => s.UserId == doctorId && s.Date.Date == date.AddDays(-1).Date);
            bool hasShiftTomorrow  = currentShifts.Any(s => s.UserId == doctorId && s.Date.Date == date.AddDays(1).Date);
            return !hasShiftYesterday && !hasShiftTomorrow;
        }

        public List<Shift> GenerateMonthlyShifts(int year, int month, List<User> doctorsInDepartment)
        {
            var generatedShifts = new List<Shift>();
            int daysInMonth = DateTime.DaysInMonth(year, month);

            var orderedDoctors = doctorsInDepartment.OrderBy(d => d.SeniorityYear).ToList();

            for (int day = 1; day <= daysInMonth; day++)
            {
                var currentDate = new DateTime(year, month, day);

                User? selectedDoctor = null;
                foreach (var doctor in orderedDoctors)
                {
                    if (CanAssignShift(doctor.Id, currentDate, generatedShifts))
                    {
                        selectedDoctor = doctor;
                        break;
                    }
                }

                if (selectedDoctor != null)
                {
                    generatedShifts.Add(new Shift
                    {
                        UserId = selectedDoctor.Id,
                        Date = currentDate
                    });
                }
            }

            return generatedShifts;
        }
    }
}