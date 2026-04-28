# 💼 ExpenseManagement — Kurumsal Harcama Yönetim Sistemi

Cloud-based SaaS modelinde çalışan, multi-tenant destekli kurumsal harcama onay platformu.

---

## 🏗️ Mimari & Topoloji

```
┌─────────────────────────────────────────────────────────┐
│                      İstemci (Swagger / HTTP)            │
└────────────────────────┬────────────────────────────────┘
                         │ JWT Bearer
          ┌──────────────▼──────────────┐
          │       Expense Service        │  :5130
          │   Onion Architecture         │
          │   CQRS + MediatR             │
          │   Repository + UnitOfWork    │
          │   Global Query Filter        │
          │   Outbox Pattern             │
          └──────┬───────────┬──────────┘
                 │ publish   │ HTTP GET (Polly)
                 │           │
          ┌──────▼──────┐   │
          │  RabbitMQ   │   │
          │   :5672     │   │
          └──────┬──────┘   │
                 │ consume  │
          ┌──────▼──────────▼──────────┐
          │    Notification Service     │  :5051
          │    MassTransit Consumers    │
          │    Mock bildirim gönderici  │
          │    Auto Token (Startup)     │
          └─────────────────────────────┘

Veritabanları:
  Expense DB      → PostgreSQL :5432 (expense_db)
  pgAdmin         → :5050
  RabbitMQ UI     → :15672
```

---

## 🚀 Projeyi Çalıştırma

### Ön Gereksinimler
- .NET 9 SDK
- Docker & Docker Compose

### 1. Altyapıyı Başlat

```bash
cd docker
docker compose up -d
```

Bu komut şunları ayağa kaldırır:
- PostgreSQL (`:5432`)
- pgAdmin (`:5050`)
- RabbitMQ (`:5672`, yönetim paneli `:15672`)

### 2. Expense Service'i Başlat

```bash
cd src/Services/Expense/Expense.API
dotnet run
```

İlk çalıştırmada migration ve seed data otomatik uygulanır.

> Swagger: http://localhost:5130/swagger

### 3. Notification Service'i Başlat

```bash
cd src/Services/Notification/ExpenseManagement.NotificationAPI
dotnet run
```

> Swagger: http://localhost:5051/swagger

> **Not:** Notification Service ayağa kalkarken Expense API'ye login isteği atarak otomatik token alır. Expense Service'in önce başlatılmış olması gerekir. Token alınamazsa servis kimliksiz çalışmaya devam eder ve konsola uyarı yazar.

---

## 👥 Test Kullanıcıları

Login için `POST /api/Auth/login` endpoint'ini kullan (şifre gerekmez):

| Email | Rol | Yetkiler |
|-------|-----|----------|
| `admin@izometri.local` | Admin | Tüm verilere erişim, 5.000 TL+ onay |
| `approver@izometri.local` | HR/Approver | Harcama onay/red, tüm talepleri görme |
| `employee@izometri.local` | Employee | Talep oluşturma, sadece kendi taleplerini görme |

### Token Kullanımı

Login sonrası dönen token'ı Swagger'da **Authorize** butonuna şu formatta gir:

```
Bearer eyJhbGci...
```

---

## 📋 İş Akışı

### 5.000 TL ve Altı
```
Personel → Talep Oluştur → HR Onaylar → ✅ Approved
```

### 5.000 TL Üzeri
```
Personel → Talep Oluştur → HR Onaylar → PendingAdminApproval → Admin Onaylar → ✅ Approved
```

### Red Durumu
```
Personel → Talep Oluştur → HR veya Admin Reddeder (min. 10 karakter red notu) → ❌ Rejected
```

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | `/api/Auth/login` | JWT token al |

### Expenses
| Method | Endpoint | Rol | Açıklama |
|--------|----------|-----|----------|
| POST | `/api/Expenses` | Employee | Harcama talebi oluştur |
| GET | `/api/Expenses` | Tümü | Harcamaları listele (rol bazlı filtreleme) |
| GET | `/api/Expenses/{id}` | Tümü | Harcama detayı |
| POST | `/api/Expenses/{id}/approve` | HR, Admin | Harcamayı onayla |
| POST | `/api/Expenses/{id}/reject` | HR, Admin | Harcamayı reddet |

### Filtreleme & Sayfalama

```
GET /api/Expenses?pageNumber=1&pageSize=10&status=2&startDate=2026-01-01&endDate=2026-12-31
```

**Status değerleri:** `1=Pending`, `2=Approved`, `3=Rejected`, `4=PendingAdminApproval`

---

## 🔧 Teknik Özellikler

### Uygulanan Gereksinimler

| Gereksinim | Durum | Açıklama |
|------------|-------|----------|
| Mikroservis Mimarisi | ✅ | Expense Service + Notification Service |
| Multi-Tenancy | ✅ | JWT claim'den TenantId, EF Core global query filter |
| Soft Delete | ✅ | IsDeleted + DeletedAt + DeletedBy, hard delete yok |
| Role-Based Authorization | ✅ | JWT claims, endpoint bazlı yetkilendirme |
| Onion Architecture | ✅ | Domain / Application / Infrastructure / API |
| Repository + Unit of Work | ✅ | Generic repository, UoW ile transaction yönetimi |
| RabbitMQ (Async) | ✅ | MassTransit, ExpenseCreated/Approved/Rejected events |
| HTTP Servisler Arası (Sync) | ✅ | Notification → Expense API, Polly retry (3 deneme) |
| Validasyon | ✅ | FluentValidation, pipeline behavior |
| EF Core + Code First | ✅ | PostgreSQL, migrations, global filters |
| JWT Authentication | ✅ | UserId + TenantId + Roles claims |
| Outbox Pattern | ✅ | MassTransit EF Core Outbox (atomik mesaj + DB işlemi) |
| Unit Testing | ✅ | xUnit + Moq + FluentAssertions |
| Integration Testing | ✅ | WebApplicationFactory + InMemory DB |
| Docker Support | ✅ | docker-compose.yml (altyapı servisleri) |
| Swagger / OpenAPI | ✅ | JWT destekli, tüm endpoint'ler dokümanlı |
| AutoMapper | ✅ | Entity → DTO dönüşümleri |
| Pipeline Behaviors | ✅ | Authorization, Validation, Logging, Performance |

### Mimari Kararlar

**Multi-Tenancy:** Her kullanıcı login olduğunda JWT token'ına `TenantId` claim'i eklenir. EF Core global query filter bu claim'i okuyarak tüm sorgulara otomatik `WHERE TenantId = @current` ekler. `User`, `ExpenseRequest` ve `Approval` entity'lerinin tamamı bu filtreye tabidir. Tek satır kod yazmadan tam izolasyon sağlanır.

**Outbox Pattern:** MassTransit'in EF Core Outbox entegrasyonu kullanılmıştır. Bu sayede veritabanı kaydı ve mesaj yayınlama aynı transaction içinde atomik olarak gerçekleşir. RabbitMQ geçici olarak çevrimdışı olsa bile mesajlar kaybolmaz, servis tekrar ayağa kalktığında iletilir.

**Onay Akışı:** 5.000 TL eşiği domain katmanında `ExpenseRequest` entity'si üzerinde yönetilir. `SendToAdminApproval()` domain metodu ile durum `PendingAdminApproval`'a geçer, ardından Admin onayı ile `Approved` olur. İş kuralları domain içinde kapsüllüdür.

**Pipeline Behaviors:** MediatR pipeline'ına 4 behavior eklenmiştir. Sırasıyla: `AuthorizationBehavior` (kullanıcı kimlik kontrolü) → `ValidationBehavior` (FluentValidation) → `LoggingBehavior` (request/response loglama) → `PerformanceBehavior` (500ms üzeri sorgular için uyarı).

**Service-to-Service Auth:** Notification Service, ayağa kalkarken Expense API'ye login isteği atarak JWT token alır ve bunu singleton olarak saklar. Bu token, Expense API'ye yapılan tüm HTTP çağrılarında kullanılır. Production ortamında OAuth2 Client Credentials flow tercih edilmelidir.

**Soft Delete:** Tüm entity'lerde `IsDeleted`, `DeletedAt`, `DeletedBy` alanları mevcuttur. EF Core global filter ile silinmiş kayıtlar otomatik olarak sorgulardan dışlanır. Fiziksel silme hiçbir şekilde yapılmaz.

---

## 🗂️ Proje Yapısı

```
ExpenseManagement/
├── docker/
│   └── docker-compose.yml
├── src/
│   ├── Shared/
│   │   ├── Shared.Contracts/              # Servisler arası event modelleri
│   │   └── Shared.Kernel/
│   └── Services/
│       ├── Expense/
│       │   ├── Expense.API/               # Controllers, Program.cs, Middleware
│       │   ├── Expense.Application/       # CQRS, Handlers, Validators, Behaviors
│       │   ├── Expense.Domain/            # Entities, Enums, AggregateRoot
│       │   └── Expense.Infrastructure/    # EF Core, Repositories, UoW, MassTransit
│       └── Notification/
│           └── ExpenseManagement.NotificationAPI/  # Consumers, HTTP Client, Token
└── tests/
    ├── Expense.UnitTests/                 # xUnit + Moq handler ve domain testleri
    └── Expense.IntegrationTests/          # WebApplicationFactory controller testleri
```

---

## 🧪 Testleri Çalıştırma

### Unit Testler

```bash
cd tests/Expense.UnitTests
dotnet test
```

### Integration Testler

```bash
cd tests/Expense.IntegrationTests
dotnet test
```

---

## ⚙️ Konfigürasyon

### Expense Service — `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=expense_db;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "SecretKey": "IzometriBilisimIcinCokGizliVeUzunBirAnahtarKelime123!!",
    "Issuer": "ExpenseAPI",
    "Audience": "ExpenseAPI"
  },
  "RabbitMq": {
    "Host": "localhost"
  }
}
```

### Notification Service — `appsettings.json`

```json
{
  "RabbitMq": {
    "Host": "localhost"
  },
  "ExpenseApi": {
    "BaseUrl": "http://localhost:5130",
    "ServiceAccountEmail": "employee@izometri.local"
  }
}
```

Notification Service başlarken `ServiceAccountEmail` ile Expense API'ye login atar ve token'ı otomatik alır. Manuel token girişi gerekmez.