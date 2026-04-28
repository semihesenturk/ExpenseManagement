using System.Net.Http.Headers;
using ExpenseManagement.NotificationAPI.Services;
using MassTransit;
using Shared.Contracts.Events;

namespace ExpenseManagement.NotificationAPI.Consumers;

public class ExpenseRejectedEventConsumer(
    ILogger<ExpenseRejectedEventConsumer> logger,
    IHttpClientFactory httpClientFactory,
    IServiceTokenProvider tokenProvider)
    : IConsumer<ExpenseRejectedEvent>
{
    public async Task Consume(ConsumeContext<ExpenseRejectedEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("❌ [Tenant: {TenantId}] {ExpenseId} ID'li harcama REDDEDİLDİ. Sebep: {Reason}. Detaylar çekiliyor...",
                message.TenantId, message.ExpenseId, message.Reason);

            var client = httpClientFactory.CreateClient("ExpenseApiClient");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenProvider.Token);

            var response = await client.GetAsync($"Expenses/{message.ExpenseId}");

            if (response.IsSuccessStatusCode)
            {
                var expenseData = await response.Content.ReadAsStringAsync();

                logger.LogInformation(
                    "✅ Red Bildirimi Hazır: {ExpenseId} ID'li harcama {RejectedBy} tarafından reddedildi. Red Sebebi: {Reason}. Harcama Verisi: {Content}",
                    message.ExpenseId, message.RejectedByUserId, message.Reason, expenseData);
            }
            else
            {
                logger.LogWarning("⚠️ Reddedilen harcama detayları Expense API'den çekilemedi. Status: {StatusCode}",
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Expense API ile iletişim hatası (ExpenseRejectedEvent).");
        }
    }
}