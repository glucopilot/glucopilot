# GlucoPilot.Mail
The GlucoPilot.Mail project is a .NET library that provides SMTP/message templating services for the GlucoPilot application. It uses the RazorRenderingEngine to render templates and MailKit/MimeKit to send messages.

## Usage
To register the mail services:
```csharp
services.AddMail(builder.Configuration.GetSection("Mail").Bind);
```

## Features

- SMTP mail sending
- Message template using Razor

## Usage

When configured and registered, the mail service can be used in conjunction with the template service to produce HTML emails:

```csharp
public record MailTemplateModel(string Name);

private readonly IMailService _mailService;
private readonly ITemplateService _templateService;

public MyService(IMailService mailService, ITemplateService templateService) {
    _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
    _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
}

var myMailModel = new MailTemplateModel() { Name = "John Doe" };
var mailRequest = new MailRequest() {
    To = ["recipient@nomail.com"],
    Subject = "Message Subject",
    Body = await _templateService.RenderTemplateAsync("Template.cshtml", myMailModel, cancellationToken).ConfigureAwait(false),
};
await _mailService.SendAsync(mailRequest, cancellationToken).ConfigureAwait(false);
````

### Templates
Templates must be stored in `{AppDomain.CurrentDomain.BaseDirectory}/Templates/{templateName}.cshtml` to be discovered by the templates service. The example template for above may look like:
```html
<h1>Hello, @Model.Name!</h1>
```