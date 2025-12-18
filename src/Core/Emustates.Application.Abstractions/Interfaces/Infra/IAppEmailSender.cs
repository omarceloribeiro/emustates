using System.Threading.Tasks;

namespace Emustates.Application.Abstractions.Interfaces.Infra
{
    public interface IAppEmailSender
    {
        /// <summary>
        /// Send a simple email to a single recipient.
        /// </summary>
        Task SendAsync(string to, string subject, string body);

        /// <summary>
        /// Send an email with optional from, cc, bcc and reply-to values. Use comma-separated lists for multiple addresses.
        /// All parameters are primitive types (strings and bool).
        /// </summary>
        Task SendAsync(
            string to,
            string subject,
            string body,
            string? from = null,
            string? cc = null,
            string? bcc = null,
            bool isHtml = false,
            string? replyTo = null);

        /// <summary>
        /// Send an email based on a template name and template data (as JSON string).
        /// Use comma-separated lists for multiple addresses.
        /// </summary>
        Task SendTemplateAsync(
            string to,
            string templateName,
            string templateDataJson,
            string? from = null,
            string? cc = null,
            string? bcc = null,
            bool isHtml = true);
    }
}
