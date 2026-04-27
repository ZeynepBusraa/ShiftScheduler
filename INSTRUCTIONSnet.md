Markdown# ShiftScheduler Proje Mimari Talimatları (.NET 8.0)



\## 📋 İçindekiler

1\. \[Genel Bakış](#genel-bakış)

2\. \[Proje Yapısı](#proje-yapısı)

3\. \[Katman Mimarisi](#katman-mimarisi)

4\. \[Kod Yazım Kuralları](#kod-yazım-kuralları)

5\. \[Yeni Özellik Ekleme Rehberi](#yeni-özellik-ekleme-rehberi)

6\. \[Örnek İş Akışı](#örnek-iş-akışı)

7\. \[MVP Başlangıç Adımları](#mvp-başlangıç-adımları)



\---



\## 🎯 Genel Bakış



Bu proje, \*\*ASP.NET Core 8\*\* tabanlı bir backend uygulamasıdır. Mimari, katmanlı (layered) yaklaşımla tasarlanmıştır ve her CRUD işlemi tek sorumluluk prensibine (SRP) göre ayrı bir \*\*Handler/Operation\*\* sınıfında tutulur (Spring Boot'taki Bean katmanının .NET karşılığı).



\### Teknoloji Stack

\- \*\*C# 12 / .NET 8\*\* (LTS sürüm)

\- \*\*ASP.NET Core Web API\*\*

\- \*\*Entity Framework Core 8\*\* (ORM, Spring Data JPA karşılığı)

\- \*\*Microsoft SQL Server\*\* (Veritabanı)

\- \*\*JWT (JSON Web Token)\*\* (Kimlik doğrulama ve yetkilendirme)

\- \*\*Serilog\*\* (Loglama, SLF4J karşılığı)

\- \*\*FluentValidation\*\* (Validasyon)

\- \*\*Swashbuckle / Swagger\*\* (API dokümantasyonu)

\- \*\*SMTP Sunucusu\*\* (E-posta bildirimleri için harici entegrasyon)



\### NuGet Paketleri

```xml

<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.\*" />

<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.\*" />

<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.\*" />

<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.\*" />

<PackageReference Include="Serilog.AspNetCore" Version="8.0.\*" />

<PackageReference Include="FluentValidation.AspNetCore" Version="11.\*" />

<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.\*" />

📁 Proje YapısıPlaintextShiftScheduler/

├── src/

│   ├── ShiftScheduler.Api/                # Web API katmanı (Controllers, Program.cs)

│   │   ├── Controllers/

│   │   ├── Middlewares/                 # Global Exception Handling

│   │   ├── Program.cs

│   │   └── appsettings.json

│   │

│   ├── ShiftScheduler.Application/        # İş mantığı katmanı

│   │   ├── Handlers/                    # CRUD operasyonları (Bean karşılığı)

│   │   │   └── Shifts/

│   │   │       ├── FindShiftHandler.cs

│   │   │       ├── ListShiftsHandler.cs

│   │   │       ├── SaveShiftHandler.cs

│   │   │       └── DeleteShiftHandler.cs

│   │   ├── Services/                    # Service interface ve implementasyonları

│   │   │   ├── IShiftService.cs

│   │   │   └── Implementations/

│   │   │       └── ShiftService.cs

│   │   ├── DTOs/                        # Data Transfer Objects

│   │   ├── Mappers/                     # Entity-DTO dönüşümleri

│   │   ├── Specifications/              # Sorgu filtreleme (Spec karşılığı)

│   │   ├── Validators/                  # FluentValidation kuralları

│   │   ├── Common/                      # ResponsePayload yapıları

│   │   └── DependencyInjection.cs

│   │

│   ├── ShiftScheduler.Domain/             # Entity ve Enum tanımları

│   │   ├── Entities/                    # User, Shift, ShiftRequest, AuditLog

│   │   ├── Enums/                       # Role, ShiftType, RequestStatus

│   │   └── Exceptions/                  # Nöbet kuralları için custom hatalar

│   │

│   └── ShiftScheduler.Infrastructure/     # Veritabanı, dış servisler

│       ├── Persistence/

│       │   ├── AppDbContext.cs

│       │   ├── Configurations/          # EF Core Fluent API yapılandırmaları

│       │   └── Migrations/

│       ├── Repositories/                # IShiftRepository vb.

│       ├── Gateways/                    # SMTP Email Servis Entegrasyonları

│       └── DependencyInjection.cs

🏗️ Katman Mimarisi1. Controller KatmanıREST API endpoint'lerini tanımlar. Her entity için ayrı controller bulunur. Constructor injection kullanılır.C#\[ApiController]

\[Route("api/\[controller]")]

\[EnableCors("DefaultPolicy")]

public class ShiftsController(IShiftService shiftService) : ControllerBase

{

&#x20;   private readonly IShiftService \_shiftService = shiftService;



&#x20;   \[HttpGet("{id}")]

&#x20;   public async Task<ResponsePayload<ShiftDto>> Find(\[FromRoute] int id)

&#x20;   {

&#x20;       return await \_shiftService.FindAsync(id);

&#x20;   }



&#x20;   \[HttpPost("save")]

&#x20;   public async Task<ResponsePayload<ShiftDto>> Save(\[FromBody] ShiftDto dto)

&#x20;   {

&#x20;       return await \_shiftService.SaveAsync(dto);

&#x20;   }



&#x20;   \[HttpGet("list")]

&#x20;   public async Task<ResponsePayload<List<ShiftDto>>> List()

&#x20;   {

&#x20;       return await \_shiftService.ListAsync();

&#x20;   }

}

2\. Service Katmanı (Facade)İş mantığı için arayüz tanımlar. Handler'ları çağırır, sadece koşullu yönlendirme yapar. Algoritmik iş mantığı içermez.Interface:C#public interface IShiftService

{

&#x20;   Task<ResponsePayload<ShiftDto>> FindAsync(int id);

&#x20;   Task<ResponsePayload<List<ShiftDto>>> ListAsync();

&#x20;   Task<ResponsePayload<ShiftDto>> SaveAsync(ShiftDto dto);

}

Implementation:C#public class ShiftService(

&#x20;   FindShiftHandler findHandler,

&#x20;   ListShiftsHandler listHandler,

&#x20;   SaveShiftHandler saveHandler) : IShiftService

{

&#x20;   private readonly FindShiftHandler \_findHandler = findHandler;

&#x20;   private readonly ListShiftsHandler \_listHandler = listHandler;

&#x20;   private readonly SaveShiftHandler \_saveHandler = saveHandler;



&#x20;   public Task<ResponsePayload<ShiftDto>> FindAsync(int id) => \_findHandler.HandleAsync(id);

&#x20;   public Task<ResponsePayload<List<ShiftDto>>> ListAsync() => \_listHandler.HandleAsync();

&#x20;   public Task<ResponsePayload<ShiftDto>> SaveAsync(ShiftDto dto) => \_saveHandler.HandleAsync(dto);

}

3\. Handler Katmanı (Bean Karşılığı)Tüm iş mantığı bu katmanda yer alır. Nöbet atama kısıtları, veritabanı kontrolleri burada yapılır.Örnek Handler:C#public class SaveShiftHandler(

&#x20;   IShiftRepository repository,

&#x20;   ShiftDtoMapper mapper,

&#x20;   FindShiftHandler findHandler,

&#x20;   ILogger<SaveShiftHandler> logger)

{

&#x20;   private readonly IShiftRepository \_repository = repository;

&#x20;   private readonly ShiftDtoMapper \_mapper = mapper;

&#x20;   private readonly FindShiftHandler \_findHandler = findHandler;

&#x20;   private readonly ILogger<SaveShiftHandler> \_logger = logger;



&#x20;   public async Task<ResponsePayload<ShiftDto>> HandleAsync(ShiftDto dto)

&#x20;   {

&#x20;       if (dto.Id > 0)

&#x20;       {

&#x20;           var existing = await \_findHandler.HandleAsync(dto.Id);

&#x20;           if (existing.Success)

&#x20;           {

&#x20;               return Response.RecordExists<ShiftDto>();

&#x20;           }

&#x20;       }



&#x20;       // TODO: Burada "Üst üste nöbet yazılamaz" (FR-02.2) iş kuralı kontrol edilecek.



&#x20;       var entity = \_mapper.ConvertToEntity(dto);

&#x20;       var saved = await \_repository.SaveAsync(entity);

&#x20;       

&#x20;       \_logger.LogInformation("Yeni nöbet eklendi. ShiftID: {Id}", saved.Id);

&#x20;       

&#x20;       return Response.SaveSuccess(\_mapper.Map(saved));

&#x20;   }

}

4\. Entity KatmanıEF Core entity sınıfları.C#public class Shift

{

&#x20;   public int Id { get; set; }

&#x20;   public int UserId { get; set; }

&#x20;   public DateTime Date { get; set; }

&#x20;   public ShiftType Type { get; set; } // Enum: Weekday, Weekend, Holiday

&#x20;   public bool IsApproved { get; set; }



&#x20;   // İlişkiler

&#x20;   public virtual User User { get; set; }

}

Fluent API Yapılandırması (Infrastructure/Persistence/Configurations/ShiftConfiguration.cs):C#public class ShiftConfiguration : IEntityTypeConfiguration<Shift>

{

&#x20;   public void Configure(EntityTypeBuilder<Shift> builder)

&#x20;   {

&#x20;       builder.ToTable("Shifts");

&#x20;       builder.HasKey(x => x.Id);

&#x20;       

&#x20;       builder.Property(x => x.Date).IsRequired();

&#x20;       

&#x20;       builder.HasOne(x => x.User)

&#x20;           .WithMany()

&#x20;           .HasForeignKey(x => x.UserId)

&#x20;           .OnDelete(DeleteBehavior.Cascade);

&#x20;   }

}

5\. DTO KatmanıC#public record ShiftDto(

&#x20;   int Id,

&#x20;   int UserId,

&#x20;   DateTime Date,

&#x20;   int ShiftType,

&#x20;   bool IsApproved

);

6\. Mapper KatmanıC#public class ShiftDtoMapper

{

&#x20;   public ShiftDto Map(Shift entity)

&#x20;   {

&#x20;       return new ShiftDto(

&#x20;           entity.Id,

&#x20;           entity.UserId,

&#x20;           entity.Date,

&#x20;           (int)entity.Type,

&#x20;           entity.IsApproved

&#x20;       );

&#x20;   }



&#x20;   public List<ShiftDto> MapList(IEnumerable<Shift> entities) => entities.Select(Map).ToList();



&#x20;   public Shift ConvertToEntity(ShiftDto dto)

&#x20;   {

&#x20;       return new Shift

&#x20;       {

&#x20;           Id = dto.Id,

&#x20;           UserId = dto.UserId,

&#x20;           Date = dto.Date,

&#x20;           Type = (ShiftType)dto.ShiftType,

&#x20;           IsApproved = dto.IsApproved

&#x20;       };

&#x20;   }

}

7\. Repository KatmanıEF Core ile repository pattern.C#public interface IShiftRepository

{

&#x20;   Task<Shift?> FindByIdAsync(int id);

&#x20;   Task<List<Shift>> ListAllAsync();

&#x20;   Task<List<Shift>> FindAllAsync(ISpecification<Shift> spec);

&#x20;   Task<Shift> SaveAsync(Shift entity);

&#x20;   Task DeleteAsync(Shift entity);

}

8\. Response YapısıTüm API yanıtları ResponsePayload<T> ile sarmalanır.C#public class ResponsePayload<T>

{

&#x20;   public bool Success { get; init; }

&#x20;   public string? Message { get; init; }

&#x20;   public string? Code { get; init; }

&#x20;   public T? Data { get; init; }

}



public static class Response

{

&#x20;   public static ResponsePayload<T> Ok<T>(T data) => new() { Success = true, Code = "OK", Data = data };

&#x20;   public static ResponsePayload<T> SaveSuccess<T>(T data) => new() { Success = true, Code = "SAVE\_SUCCESS", Message = "Kayıt başarılı", Data = data };

&#x20;   public static ResponsePayload<T> RecordExists<T>() => new() { Success = false, Code = "RECORD\_EXISTS", Message = "Kayıt zaten mevcut" };

}

📝 Kod Yazım Kurallarıİsimlendirme: Sınıflar PascalCase, private field'lar \_camelCase olmalıdır.Hardcoded Değer Yok: Rol atamaları ve nöbet tipleri için Enum kullanılmalıdır.Tek Sorumluluk: Algoritma hesaplamaları ayrı servislerde/iş mantığı sınıflarında (Handlers) yapılmalıdır.Loglama: Her kritik CRUD işlemi ve nöbet değişim onayı ILogger kullanılarak (tercihen Serilog ile AuditLog tablosuna) kaydedilmelidir.Async/Await: Veritabanı işlemlerinde kesinlikle Task döndürülmeli ve .ToListAsync(), .FirstOrDefaultAsync() kullanılmalıdır.🚀 Yeni Özellik Ekleme Rehberi (Örn: ShiftRequest)Entity: Domain/Entities/ShiftRequest.csConfig: Infrastructure/Persistence/Configurations/ShiftRequestConfiguration.csDTO: Application/DTOs/ShiftRequestDto.csMapper: Application/Mappers/ShiftRequestDtoMapper.csRepository: IRepository/IShiftRequestRepository.cs -> ShiftRequestRepository.csHandlers: Application/Handlers/ShiftRequests/CreateShiftRequestHandler.csApproveShiftRequestHandler.csService: IShiftRequestService.cs -> ShiftRequestService.csController: Api/Controllers/ShiftRequestsController.csDI Kaydı: DependencyInjection.cs dosyalarına ekleme yapın.🔄 Örnek İş AkışıPOST /api/shifts/save isteği geldiğinde:PlaintextClient Request (ShiftDto)

&#x20;   ↓

ShiftsController.Save(dto)

&#x20;   ↓

IShiftService.SaveAsync(dto)

&#x20;   ↓

SaveShiftHandler.HandleAsync(dto)

&#x20;   ↓

ShiftDtoMapper.ConvertToEntity(dto)

&#x20;   ↓

IShiftRepository.SaveAsync(entity)

&#x20;   ↓

DbContext.SaveChangesAsync()

&#x20;   ↓

ShiftDtoMapper.Map(savedEntity)

&#x20;   ↓

Response.SaveSuccess(dto)

&#x20;   ↓

Client Response (ResponsePayload<ShiftDto>)

🛠️ MVP Başlangıç AdımlarıSolution ve Projeleri Oluştur:Bashdotnet new sln -n ShiftScheduler

dotnet new webapi -n ShiftScheduler.Api

dotnet new classlib -n ShiftScheduler.Application

dotnet new classlib -n ShiftScheduler.Domain

dotnet new classlib -n ShiftScheduler.Infrastructure

\# Projeleri solution'a ekle ve referansları bağla.

Gerekli Paketleri Kur: (EF Core, SQL Server, Serilog).appsettings.json Yapılandırması: SQL Server Connection string'ini ekle.İlk Migration ve Update:Bashdotnet ef migrations add InitialCreate --project ShiftScheduler.Infrastructure --startup-project ShiftScheduler.Api

dotnet ef database update --project ShiftScheduler.Infrastructure --startup-project ShiftScheduler.Api

📚 Spring Boot → .NET Hızlı Karşılaştırma TablosuSpring Boot.NET / ASP.NET Core@RestController\[ApiController]@RequestMapping("/path")\[Route("path")]@GetMapping, @PostMapping\[HttpGet], \[HttpPost]@RequestParam\[FromQuery]@RequestBody\[FromBody]@PathVariable\[FromRoute]@Service, @ComponentDI ile services.AddScoped<>()@RepositoryEF Core + DbContext@Autowired / @RequiredArgsConstructorConstructor injection / Primary Constructor@TransactionalDbContext.Database.BeginTransactionAsync()@Value("${...}")IConfiguration\["..."]Spring Data JPAEntity Framework CoreLombok @Builder/@Getterrecord veya init-only properties

