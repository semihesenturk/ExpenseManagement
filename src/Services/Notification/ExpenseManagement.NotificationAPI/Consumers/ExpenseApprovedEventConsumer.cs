using System.Net.Http.Headers;
using MassTransit;
using Shared.Contracts.Events;

namespace Notification.API.Consumers;

public class ExpenseApprovedEventConsumer : IConsumer<ExpenseApprovedEvent>
{
    private readonly ILogger<ExpenseApprovedEventConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ExpenseApprovedEventConsumer(
        ILogger<ExpenseApprovedEventConsumer> logger, 
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<ExpenseApprovedEvent> context)
    {
        var message = context.Message;

        try
        {
            _logger.LogInformation("🚀 [Tenant: {TenantId}] {ExpenseId} ID'li harcama ONAYLANDI. Detaylar çekiliyor...", message.TenantId, message.ExpenseId);

            // Swagger'dan aldığın güncel token
            var token = _configuration["TestToken"];
            
            var client = _httpClientFactory.CreateClient("ExpenseApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Expense API'den onaylanmış harcamanın son halini çekiyoruz
            var response = await client.GetAsync($"Expenses/{message.ExpenseId}");

            if (response.IsSuccessStatusCode)
            {
                // DTO'na göre burayı düzenleyebilirsin
                var expense = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("✅ Onay Bildirimi Hazır: {ExpenseId} ID'li harcama {ApprovedBy} tarafından onaylandı. Veri: {Content}", 
                    message.ExpenseId, message.ApprovedByUserId, expense);
                
                // Burada Email veya Push servislerini tetikleyebilirsin
            }
            else
            {
                _logger.LogWarning("⚠️ Onaylı harcama detayları çekilemedi. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Expense API ile iletişim hatası (ExpenseApprovedEvent).");
        }
    }
}