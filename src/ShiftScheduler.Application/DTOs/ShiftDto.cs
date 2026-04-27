using System;

namespace ShiftScheduler.Application.DTOs;

public record ShiftDto(
    int Id,
    int UserId,
    DateTime Date,
    int ShiftType,
    bool IsApproved
);
