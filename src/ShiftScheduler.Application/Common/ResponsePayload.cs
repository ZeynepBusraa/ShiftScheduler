namespace ShiftScheduler.Application.Common;

public class ResponsePayload<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? Code { get; init; }
    public T? Data { get; init; }
}

public static class Response
{
    public static ResponsePayload<T> Ok<T>(T data) => new() { Success = true, Code = "OK", Data = data };
    public static ResponsePayload<T> SaveSuccess<T>(T data) => new() { Success = true, Code = "KAYIT_BASARILI", Message = "Kayıt başarıyla tamamlandı.", Data = data };
    public static ResponsePayload<T> RuleViolation<T>(string message) => new() { Success = false, Code = "KURAL_IHLALI", Message = message };
}
