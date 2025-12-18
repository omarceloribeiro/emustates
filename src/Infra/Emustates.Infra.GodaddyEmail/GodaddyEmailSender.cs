using Emustates.Application.Abstractions.Interfaces.Infra;

namespace Emustates.Infra.GodaddyEmail
{
    internal class GodaddyEmailSender : IAppEmailSender
    {
        public Task SendAsync(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(string to, string subject, string body, string? from = null, string? cc = null, string? bcc = null, bool isHtml = false, string? replyTo = null)
        {
            throw new NotImplementedException();
        }

        public Task SendTemplateAsync(string to, string templateName, string templateDataJson, string? from = null, string? cc = null, string? bcc = null, bool isHtml = true)
        {
            throw new NotImplementedException();
        }
    }
}
