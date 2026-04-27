# ShiftScheduler 🏥

Klinik Nöbet Otomasyonu — Asistan doktorların nöbet çizelgelerini adil, çakışmasız ve şeffaf biçimde yönetmek için geliştirilmiş web tabanlı sistem.

## Teknoloji Yığını

- **Backend:** ASP.NET Core 8 (N-Tier Mimari)
- **ORM:** Entity Framework Core
- **Auth:** JWT Bearer Token
- **Veritabanı:** SQL Server (bulut: Neon.tech / Somee)

## Proje Yapısı

```
ShiftScheduler/
├── src/
│   ├── ShiftScheduler.Domain          # Entity'ler, Enum'lar
│   ├── ShiftScheduler.Application     # Servisler, Handler'lar, DTO'lar, Repository arayüzleri
│   ├── ShiftScheduler.Infrastructure  # EF Core, Repository implementasyonları, JWT
│   └── ShiftScheduler.Api             # Controller'lar, Program.cs
```

## Kurulum

### 1. Repoyu Klonla

```bash
git clone https://github.com/KULLANICI_ADI/ShiftScheduler.git
cd ShiftScheduler
```

### 2. Bağlantı Ayarlarını Yapılandır

`appsettings.Example.json` dosyasını kopyala ve gerçek değerlerini gir:

```bash
cp src/ShiftScheduler.Api/appsettings.Example.json src/ShiftScheduler.Api/appsettings.json
```

`appsettings.json` içini doldur:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "GERÇEK_VERİTABANI_BAĞLANTI_DİZESİ"
  },
  "JwtSettings": {
    "SecretKey": "EN_AZ_32_KARAKTER_GIZLI_ANAHTAR",
    "Issuer": "ShiftSchedulerApi",
    "Audience": "ShiftSchedulerClients",
    "ExpirationMinutes": 30
  }
}
```

### 3. Migration Uygula ve Çalıştır

```bash
dotnet restore
dotnet run --project src/ShiftScheduler.Api
```

> Migration'lar uygulama başlangıcında otomatik uygulanır.

## API Endpoint'leri

| Method | Endpoint | Açıklama | Yetki |
|--------|----------|----------|-------|
| POST | `/api/auth/login` | Kullanıcı girişi, JWT döner | Herkese açık |
| GET | `/api/shifts/list` | Nöbet listesi (role göre filtreli) | Giriş gerekli |
| POST | `/api/shifts/save` | Nöbet kaydet/güncelle | Giriş gerekli |
| GET | `/api/shifts/{id}` | Tek nöbet getir | Giriş gerekli |
| POST | `/api/users` | Yeni kullanıcı ekle | Başhekim/Admin |
| GET | `/api/users` | Kullanıcı listesi | Başhekim/Admin |
| DELETE | `/api/users/{id}` | Kullanıcı sil | Başhekim/Admin |

## Ekip

- Hüseyin Fidan / 22118080002
- Mert Yüce Daşbacak / 22118080040
- Zeynep Büşra Yılmaz / 22118080076
