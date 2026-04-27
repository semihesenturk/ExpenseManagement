using System.Net.Http.Headers;
using MassTransit;
using Shared.Contracts.Events;

namespace Notification.API.Consumers;

public class ExpenseCreatedEventConsumer : IConsumer<ExpenseCreatedEvent>
{
    private readonly ILogger<ExpenseCreatedEventConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ExpenseCreatedEventConsumer(
        ILogger<ExpenseCreatedEventConsumer> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Consume(ConsumeContext<ExpenseCreatedEvent> context)
    {
        var message = context.Message;
        
        try
        {
            _logger.LogInformation("⏳ [Tenant: {TenantId}] {ExpenseId} ID'li harcama yakalandı. Detaylar Expense API'den çekiliyor...", message.TenantId, message.ExpenseId);

            // TR-7: Senkron HTTP Çağrısı
            var client = _httpClientFactory.CreateClient("ExpenseApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImRkYmVjNTIxLTk1NjYtNDAzYS05NGYzLTJiNGJhZmIwYmFhZSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImVtcGxveWVlQGl6b21ldHJpLmxvY2FsIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiRW1wbG95ZWUiLCJUZW5hbnRJZCI6IjZkYzI5YzNjLWQ5NDUtNGY0MC05YWNjLWNmNDQ3ZGE1ZWI5OCIsImV4cCI6MTc3NzMwNDY0OSwiaXNzIjoiRXhwZW5zZUFQSSIsImF1ZCI6IkV4cGVuc2VBUEkifQ.1KNhctdjkvM2HFfb1TqiaPorVRg_XS1AcG_rgGoVUdY");
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