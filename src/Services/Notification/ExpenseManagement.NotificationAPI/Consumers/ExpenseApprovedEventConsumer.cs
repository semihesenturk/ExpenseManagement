using System.Net.Http.Headers;
using ExpenseManagement.NotificationAPI.Services;
using MassTransit;
using Shared.Contracts.Events;

namespace ExpenseManagement.NotificationAPI.Consumers;

public class ExpenseApprovedEventConsumer(
    ILogger<ExpenseApprovedEventConsumer> logger,
    IHttpClientFactory httpClientFactory,
    IServiceTokenProvider tokenProvider)
    : IConsumer<ExpenseApprovedEvent>
{
    public async Task Consume(ConsumeContext<ExpenseApprovedEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("🚀 [Tenant: {TenantId}] {ExpenseId} ID'li harcama ONAYLANDI. Detaylar çekiliyor...",
                message.TenantId, message.ExpenseId);

            var client = httpClientFactory.CreateClient("ExpenseApiClient");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenProvider.Token);

            var response = await client.GetAsync($"Expenses/{message.ExpenseId}");

            if (response.IsSuccessStatusCode)
            {
                var expense = await response.Content.ReadAsStringAsync();

                logger.LogInformation(
                    "✅ Onay Bildirimi Hazır: {ExpenseId} ID'li harcama {ApprovedBy} tarafından onaylandı. Veri: {Content}",
                    message.ExpenseId, message.ApprovedByUserId, expense);
            }
            else
            {
                logger.LogWarning("⚠️ Onaylı harcama detayları çekilemedi. Status: {StatusCode}",
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Expense API ile iletişim hatası (ExpenseApprovedEvent).");
        }
    }
}