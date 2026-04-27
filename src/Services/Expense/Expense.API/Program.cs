using System.Text;
using Expense.Application;
using Expense.Application.Common.Interfaces;
using Expense.Infrastructure;
using Expense.Infrastructure.Persistence.SeedData;
using Expense.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Katman Kayıtları (Layers)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. HTTP Context Erişimi (Kritik: ICurrentUserService'in çalışması için şart!)
builder.Services.AddHttpContextAccessor();

// 3. Kendi Servisimiz (Infrastructure içindeki kaydı ezmemesi için AddInfrastructure'dan SONRA ekledik)
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 4. API Servisleri
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 5. Swagger Konfigürasyonu (JWT Destekli)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Izometri ExpenseManagement API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Token'ınızı buraya girin. Örnek: Bearer eyJhbGci...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 6. Kimlik Doğrulama ve Yetkilendirme (Auth)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "ExpenseAPI",
            ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "ExpenseAPI",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "IzometriBilisimIcinCokGizliVeUzunBirAnahtarKelime123!!"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 7. Middleware Akışı (Pipeline)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

// 8. Veritabanı Seed İşlemi
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await ExpenseDbContextSeed.SeedAsync(services);
        Console.WriteLine("✅ Veritabanı seed işlemi başarıyla tamamlandı.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seed data oluşturulurken bir hata oluştu: {ex.Message}");
    }
}

if (!builder.Environment.IsEnvironment("Testing"))
{
    // Gerçek PostgreSQL kaydı sadece "Testing" ortamında DEĞİLSE çalışacak
    builder.Services.AddDbContext<ExpenseDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

app.Run();

//For integration tests
public partial class Program { }