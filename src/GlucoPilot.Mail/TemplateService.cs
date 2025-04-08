using System.Text;
using RazorEngineCore;

namespace GlucoPilot.Mail;

internal sealed class TemplateService : ITemplateService
{
    public async Task<string> RenderTemplateAsync<T>(string templateName, T model,
        CancellationToken cancellationToken = default)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var templatePath = Path.Combine(baseDirectory, "Templates", $"{templateName}.cshtml");
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template '{templateName}' not found.", templatePath);
        }

        var stream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using (stream.ConfigureAwait(false))
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var templateContent = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            var razorEngine = new RazorEngine();
            var compiledTemplate = await razorEngine.CompileAsync(templateContent, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return await compiledTemplate.RunAsync(model).ConfigureAwait(false);
        }
    }
}