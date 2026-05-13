// src/ShiftScheduler.Domain/Enums/RequestStatus.cs
namespace ShiftScheduler.Domain.Enums;

/// <summary>
/// Nöbet değişim talebinin durum geçişleri:
/// Bekliyor → (hedef doktor kabul) → HedefOnayladi → (en kıdemli onay) → KidemliOnayladi → (başhekim onay) → BashekimOnayladi
/// Herhangi bir aşamada red → Reddedildi
/// </summary>
public enum RequestStatus
{
    Bekliyor = 0,
    HedefOnayladi = 1,   // Hedef doktor kabul etti; en kıdemlinin onayı bekleniyor
    KidemliOnayladi = 2, // En kıdemli asistan/uzman onayladı; başhekimin onayı bekleniyor
    BashekimOnayladi = 3,// Kesinleşti
    Reddedildi = 4
}