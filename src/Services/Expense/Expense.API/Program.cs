using System.Text;
using Expense.Application;
using Expense.Infrastructure;
using Expense.Infrastructure.Persistence.SeedData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//Add Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExpenseManagement API", Version = "v1" });
    
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

//Add jwt
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "IzometriSaaS",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "IzometriSaaS",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "super-secret-key-for-izometri-case-study-needs-to-be-long"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

//Seed initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Namespace'e dikkat: Expense.Infrastructure.Persistence.ExpenseDbContextSeed
        await ExpenseDbContextSeed.SeedAsync(services);
        Console.WriteLine("✅ Veritabanı seed işlemi başarıyla tamamlandı.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seed data oluşturulurken bir hata oluştu: {ex.Message}");
    }
}

app.Run();