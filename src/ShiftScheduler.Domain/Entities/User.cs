// src/ShiftScheduler.Domain/Entities/User.cs
using System;
using System.Collections.Generic;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
    public int SeniorityYear { get; set; }

    /// <summary>Ad ve soyadı birleştirir. Mapper ve handler'larda kullanılır.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    // YENİ İŞ KURALLARI İÇİN EKLENEN ALANLAR
    public bool IsSenior { get; set; } = false; // En kıdemli asistan/uzman yetkisi
    public int RemainingChangeRequests { get; set; } = 2; // Her ay başı 2 olarak güncellenecek

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int FailedLoginCount { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }

    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public ICollection<ShiftList> PreparedShiftLists { get; set; } = new List<ShiftList>();
}