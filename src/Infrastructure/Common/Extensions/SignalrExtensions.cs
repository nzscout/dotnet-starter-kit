using DN.WebApi.Application.Settings;
using DN.WebApi.Infrastructure.Multitenancy;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DN.WebApi.Infrastructure.Common.Extensions;

public static class SignalRExtensions
{
    internal static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        ILogger logger = Log.ForContext(typeof(SignalRExtensions));

        var signalSettings = services.GetOptions<SignalRSettings>("SignalRSettings");

        if (!signalSettings.UseBackplane)
        {
            services.AddSignalR();
        }
        else
        {
            var backplaneSettings = services.GetOptions<SignalRSettings.Backplane>("SignalRSettings:Backplane");
            switch (backplaneSettings.Provider)
            {
                case "redis":
                    services.AddSignalR().AddStackExchangeRedis(backplaneSettings.StringConnection, options =>
                    {
                        options.Configuration.AbortOnConnectFail = false;
                    });
                    break;

                default:
                    throw new Exception($"SignalR backplane Provider {backplaneSettings.Provider} is not supported.");
            }

            logger.Information($"SignalR Backplane Current Provider: {backplaneSettings.Provider}.");
        }

        return services;
    }
}