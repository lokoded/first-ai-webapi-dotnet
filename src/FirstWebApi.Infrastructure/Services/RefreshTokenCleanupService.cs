using FirstWebApi.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FirstWebApi.Infrastructure.Services;

public class RefreshTokenCleanupService(IServiceProvider serviceProvider, ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);
    private const int KeepDays = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                var removed = await repo.DeleteExpiredAsync(KeepDays, stoppingToken);
                if (removed > 0)
                    logger.LogInformation("Limpeza de refresh tokens: {Count} removidos", removed);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao limpar refresh tokens expirados");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
