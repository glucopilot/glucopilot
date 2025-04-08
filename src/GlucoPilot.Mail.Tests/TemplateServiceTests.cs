namespace GlucoPilot.Mail.Tests;

[TestFixture]
internal sealed class TemplateServiceTests
{
    private TemplateService _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new TemplateService();
    }

    [Test]
    public void RenderTemplateAsync_TemplateNotFound_ThrowsFileNotFoundException()
    {
        const string templateName = "NonExistentTemplate";
        var model = new { Name = "John Doe" };

        Assert.That(() => _sut.RenderTemplateAsync(templateName, model), Throws.InstanceOf<FileNotFoundException>());
    }

    [Test]
    public async Task RenderTemplateAsync_ValidTemplate_ReturnsRenderedTemplate()
    {
        const string templateName = "Test";
        var model = new { Name = "John Doe" };
        const string expectedOutput = "<h1>Hello, John Doe!</h1>";

        var result = await _sut.RenderTemplateAsync(templateName, model);

        Assert.That(result, Is.EqualTo(expectedOutput));
    }
}