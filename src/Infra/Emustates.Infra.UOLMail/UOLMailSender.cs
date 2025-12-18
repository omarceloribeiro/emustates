using Emustates.Application.Abstractions.Interfaces.Infra;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Emustates.Infra.UOLMail
{
    public class UOLMailSender : IAppEmailSender
    {
        private readonly UOLMailOptions _options;
        private readonly ILogger<UOLMailSender> _logger;

        public UOLMailSender(IOptions<UOLMailOptions> options, ILogger<UOLMailSender> logger)
        {
            _options = options?.Value ?? new UOLMailOptions();
            _logger = logger;
        }

        public Task SendAsync(string to, string subject, string body)
        {
            return SendAsync(to, subject, body, from: null, cc: null, bcc: null, isHtml: false, replyTo: null);
        }

        public async Task SendAsync(string to, string subject, string body, string? from = null, string? cc = null, string? bcc = null, bool isHtml = false, string? replyTo = null)
        {
            _logger.LogDebug("Preparing email. To: {To}, Subject: {Subject}", to, subject);

            using var message = new MailMessage();

            // From
            if (!string.IsNullOrWhiteSpace(from))
            {
                message.From = new MailAddress(from);
            }
            else if (!string.IsNullOrWhiteSpace(_options.From))
            {
                if (!string.IsNullOrWhiteSpace(_options.FromName))
                    message.From = new MailAddress(_options.From, _options.FromName);
                else
                    message.From = new MailAddress(_options.From);
            }

            if (message.From == null)
            {
                _logger?.LogWarning("Email send aborted: no From address provided (neither method parameter nor options).");
                return;
            }

            // To (can be comma-separated)
            AddAddresses(message.To, to);

            // CC and BCC
            if (!string.IsNullOrWhiteSpace(cc)) AddAddresses(message.CC, cc);
            if (!string.IsNullOrWhiteSpace(_options.DefaultCc)) AddAddresses(message.CC, _options.DefaultCc);
            if (!string.IsNullOrWhiteSpace(bcc)) AddAddresses(message.Bcc, bcc);
            if (!string.IsNullOrWhiteSpace(_options.DefaultBcc)) AddAddresses(message.Bcc, _options.DefaultBcc);

            // ReplyTo
            if (!string.IsNullOrWhiteSpace(replyTo))
            {
                try { message.ReplyToList.Add(new MailAddress(replyTo)); } catch (System.Exception ex) { _logger?.LogWarning(ex, "Invalid reply-to address provided: {ReplyTo}", replyTo); }
            }
            else if (!string.IsNullOrWhiteSpace(_options.ReplyTo))
            {
                try { message.ReplyToList.Add(new MailAddress(_options.ReplyTo)); } catch (System.Exception ex) { _logger?.LogWarning(ex, "Invalid reply-to address in options: {ReplyTo}", _options.ReplyTo); }
            }

            if (message.To.Count == 0 && message.CC.Count == 0 && message.Bcc.Count == 0)
            {
                _logger?.LogWarning("Email send aborted: no recipients specified (To/CC/Bcc are empty or invalid). To parameter: {To}", to);
                return;
            }

            message.Subject = subject ?? string.Empty;
            message.Body = body ?? string.Empty;
            message.IsBodyHtml = isHtml;

            using var client = CreateSmtpClient();

            try
            {
                await client.SendMailAsync(message).ConfigureAwait(false);
                _logger?.LogInformation("Email sent successfully. From: {From}, To: {ToList}, Subject: {Subject}", message.From?.Address, GetAddressesAsString(message.To), message.Subject);
            }
            catch (SmtpFailedRecipientsException ex)
            {
                _logger?.LogError(ex, "Failed sending email to one or more recipients. To: {To}", to);
                throw;
            }
            catch (SmtpException ex)
            {
                _logger?.LogError(ex, "SMTP error when sending email. To: {To}, Subject: {Subject}", to, subject);
                throw;
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error when sending email. To: {To}, Subject: {Subject}", to, subject);
                throw;
            }
        }

        public Task SendTemplateAsync(string to, string templateName, string templateDataJson, string? from = null, string? cc = null, string? bcc = null, bool isHtml = true)
        {
            _logger?.LogDebug("Preparing template email. Template: {Template}, To: {To}", templateName, to);

            // Simple template handling: include template name and data in body when no template engine is available.
            var subject = templateName ?? "";
            var body = $"Template: {templateName}\nData: {templateDataJson}";

            return SendAsync(to, subject, body, from: from, cc: cc, bcc: bcc, isHtml: isHtml, replyTo: null);
        }

        private SmtpClient CreateSmtpClient()
        {
            try
            {
                var client = new SmtpClient
                {
                    Host = _options.Host,
                    Port = _options.Port,
                    EnableSsl = _options.EnableSsl,
                    UseDefaultCredentials = _options.UseDefaultCredentials,
                    Timeout = _options.TimeoutMilliseconds
                };

                if (!string.IsNullOrWhiteSpace(_options.Username))
                {
                    client.Credentials = new NetworkCredential(_options.Username, _options.Password);
                }

                // Delivery method
                if (!string.IsNullOrWhiteSpace(_options.DeliveryMethod) && _options.DeliveryMethod.Equals("SpecifiedPickupDirectory", System.StringComparison.OrdinalIgnoreCase))
                {
                    client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    if (!string.IsNullOrWhiteSpace(_options.PickupDirectoryLocation))
                    {
                        client.PickupDirectoryLocation = _options.PickupDirectoryLocation;
                    }
                }
                else
                {
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                }

                _logger?.LogDebug("SMTP client created. Host: {Host}, Port: {Port}, DeliveryMethod: {DeliveryMethod}", _options.Host, _options.Port, client.DeliveryMethod);

                return client;
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Failed to create SMTP client with provided options.");
                throw;
            }
        }

        private void AddAddresses(MailAddressCollection collection, string addressesCsv)
        {
            if (string.IsNullOrWhiteSpace(addressesCsv)) return;

            var separators = new[] { ',', ';' };
            var parts = addressesCsv.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var trimmed = p.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;
                try
                {
                    collection.Add(new MailAddress(trimmed));
                }
                catch (System.Exception ex)
                {
                    _logger?.LogWarning(ex, "Ignored invalid email address: {Address}", trimmed);
                }
            }
        }

        private static string GetAddressesAsString(MailAddressCollection collection)
        {
            if (collection == null || collection.Count == 0) return string.Empty;
            var list = new System.Collections.Generic.List<string>(collection.Count);
            foreach (var a in collection)
            {
                list.Add(a.Address);
            }
            return string.Join(",", list);
        }
    }
}
