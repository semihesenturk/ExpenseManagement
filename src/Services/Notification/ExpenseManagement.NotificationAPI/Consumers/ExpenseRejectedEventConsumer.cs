using System.Net.Http.Headers;
using MassTransit;
using Shared.Contracts.Events;

namespace Notification.API.Consumers;

public class ExpenseRejectedEventConsumer : IConsumer<ExpenseRejectedEvent>
{
    private readonly ILogger<ExpenseRejectedEventConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ExpenseRejectedEventConsumer(
        ILogger<ExpenseRejectedEventConsumer> logger, 
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<ExpenseRejectedEvent> context)
    {
        var message = context.Message;

        try
        {
            _logger.LogInformation("❌ [Tenant: {TenantId}] {ExpenseId} ID'li harcama REDDEDİLDİ. Sebep: {Reason}. Detaylar çekiliyor...", 
                message.TenantId, message.ExpenseId, message.Reason);
            
            var token = _configuration["TestToken"];
            
            var client = _httpClientFactory.CreateClient("ExpenseApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await client.GetAsync($"Expenses/{message.ExpenseId}");

            if (response.IsSuccessStatusCode)
            {
                var expenseData = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("✅ Red Bildirimi Hazır: {ExpenseId} ID'li harcama {RejectedBy} tarafından reddedildi. Red Sebebi: {Reason}. Harcama Verisi: {Content}", 
                    message.ExpenseId, message.RejectedByUserId, message.Reason, expenseData);
                
            }
            else
            {
                _logger.LogWarning("⚠️ Reddedilen harcama detayları Expense API'den çekilemedi. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Expense API ile iletişim hatası (ExpenseRejectedEvent).");
        }
    }
}