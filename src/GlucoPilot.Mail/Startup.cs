using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoPilot.Mail;

[ExcludeFromCodeCoverage]
public static class Startup
{
    public static IServiceCollection AddMail(this IServiceCollection services, Action<MailOptions> configure)
    {
        services.AddOptions<MailOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<IMailService, MailService>();
        services.AddTransient<ISmtpClientFactory, SmtpClientFactory>();

        return services;
    }
}