namespace ShiftScheduler.Domain.Enums;

/// <summary>
/// Nöbet listesinin hangi personel grubuna ait olduğunu belirtir.
/// Asistan ve Uzman listeleri birbirinden ayrı tutulur.
/// </summary>
public enum ShiftListType
{
    Asistan = 0,
    Uzman = 1
}
