using Emustates.Infra.Data;
using Emustates.Site.Components.Account;
using Emustates.Site.Data;
using Microsoft.AspNetCore.Identity;

namespace Emustates.Site
{
    public static class DependencyInjectionExtensions
    {
        public static TBuilder AddAspnetIdentityImplementation<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
            return builder;
        }
    }
}
