using System;
using System.Collections.Generic;
using System.Text;

namespace Emustates.Infra.UOLMail
{
    public class UOLMailOptions
    {
        /// <summary>
        /// SMTP server host (e.g. "smtp.example.com").
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// SMTP server port (e.g. 25, 587, 465).
        /// </summary>
        public int Port { get; set; } = 25;

        /// <summary>
        /// Use SSL/TLS when connecting to the SMTP server.
        /// </summary>
        public bool EnableSsl { get; set; } = false;

        /// <summary>
        /// Use the STARTTLS extension if supported by the server.
        /// </summary>
        public bool UseStartTls { get; set; } = false;

        /// <summary>
        /// Username for SMTP authentication.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password for SMTP authentication.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Default from address used when no from is provided.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Default display name for the From address.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Default reply-to address.
        /// </summary>
        public string ReplyTo { get; set; }

        /// <summary>
        /// Use the system's default credentials for authentication.
        /// </summary>
        public bool UseDefaultCredentials { get; set; } = false;

        /// <summary>
        /// Timeout in milliseconds for SMTP operations.
        /// </summary>
        public int TimeoutMilliseconds { get; set; } = 100000;

        /// <summary>
        /// Delivery method fallback (e.g. "Network", "SpecifiedPickupDirectory").
        /// Stored as string to keep primitive types only.
        /// </summary>
        public string DeliveryMethod { get; set; } = "Network";

        /// <summary>
        /// Path to pickup directory when DeliveryMethod is "SpecifiedPickupDirectory".
        /// </summary>
        public string PickupDirectoryLocation { get; set; }

        /// <summary>
        /// Comma-separated list of default CC addresses.
        /// </summary>
        public string DefaultCc { get; set; }

        /// <summary>
        /// Comma-separated list of default BCC addresses.
        /// </summary>
        public string DefaultBcc { get; set; }
    }
}
