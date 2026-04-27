using System.Net.Http.Headers;
using MassTransit;
using Shared.Contracts.Events;

namespace Notification.API.Consumers;

public class ExpenseCreatedEventConsumer : IConsumer<ExpenseCreatedEvent>
{
    private readonly ILogger<ExpenseCreatedEventConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ExpenseCreatedEventConsumer(
        ILogger<ExpenseCreatedEventConsumer> logger,
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<ExpenseCreatedEvent> context)
    {
        var message = context.Message;
        
        try
        {
            _logger.LogInformation("⏳ [Tenant: {TenantId}] {ExpenseId} ID'li harcama yakalandı. Detaylar Expense API'den çekiliyor...", message.TenantId, message.ExpenseId);

            // TR-7: Senkron HTTP Çağrısı
            var token = _configuration["TestToken"];
            
            var client = _httpClientFactory.CreateClient("ExpenseApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"Expenses/{message.ExpenseId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ Detaylar başarıyla çekildi: {Content}. HR departmanına mail atılıyor...", content);
            }
            else
            {
                _logger.LogWarning("⚠️ Expense API'den detay çekilemedi. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Expense API'ye bağlanırken hata oluştu (Polly tüm denemeleri yaptı ancak servis ayakta değil).");
        }
    }
}