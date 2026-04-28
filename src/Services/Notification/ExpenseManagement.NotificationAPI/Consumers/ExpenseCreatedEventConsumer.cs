using System.Net.Http.Headers;
using ExpenseManagement.NotificationAPI.Services;
using MassTransit;
using Shared.Contracts.Events;

namespace ExpenseManagement.NotificationAPI.Consumers;

public class ExpenseCreatedEventConsumer(
    ILogger<ExpenseCreatedEventConsumer> logger,
    IHttpClientFactory httpClientFactory,
    IServiceTokenProvider tokenProvider)
    : IConsumer<ExpenseCreatedEvent>
{
    public async Task Consume(ConsumeContext<ExpenseCreatedEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("⏳ [Tenant: {TenantId}] {ExpenseId} ID'li harcama yakalandı. Detaylar Expense API'den çekiliyor...",
                message.TenantId, message.ExpenseId);

            var client = httpClientFactory.CreateClient("ExpenseApiClient");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenProvider.Token);

            var response = await client.GetAsync($"Expenses/{message.ExpenseId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                logger.LogInformation("✅ Detaylar başarıyla çekildi: {Content}. HR departmanına mail atılıyor...", content);
            }
            else
            {
                logger.LogWarning("⚠️ Expense API'den detay çekilemedi. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Expense API'ye bağlanırken hata oluştu (Polly tüm denemeleri yaptı ancak servis ayakta değil).");
        }
    }
}