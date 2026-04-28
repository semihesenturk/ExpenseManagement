namespace ExpenseManagement.NotificationAPI.Services;

public class ServiceTokenProvider(string token) : IServiceTokenProvider
{
    public string Token { get; } = token;
}