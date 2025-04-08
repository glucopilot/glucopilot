namespace GlucoPilot.Mail;

public interface ITemplateService
{
    Task<string> RenderTemplateAsync<T>(string templateName, T model, CancellationToken cancellationToken = default);
}