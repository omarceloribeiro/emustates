using Emustates.Site.Data;
using Microsoft.AspNetCore.Identity;

namespace Emustates.Site.IdentityCore
{
    public class IdentityEmailSender : IEmailSender<ApplicationUser>
    {
        public IdentityEmailSender()
        {
            
        }
        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            throw new NotImplementedException();
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            throw new NotImplementedException();
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            throw new NotImplementedException();
        }
    }
}
