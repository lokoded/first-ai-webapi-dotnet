using FirstWebApi.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FirstWebApi.Infrastructure.Services;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);
    private const int KeepDays = 30;

    public RefreshTokenCleanupService(IServiceProvider serviceProvider, ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                var removed = await repo.DeleteExpiredAsync(KeepDays);
                if (removed > 0)
                    _logger.LogInformation("Limpeza de refresh tokens: {Count} removidos", removed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar refresh tokens expirados");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
