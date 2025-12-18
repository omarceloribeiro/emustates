using Emustates.Infra.Data;
using Emustates.Site.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Emustates.Site.IdentityCore
{
    public static class DependencyInjectionExtensions
    {
        public static TBuilder AddAspnetIdentityCore<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
           .AddEntityFrameworkStores<EmustatesDbContext>()
           .AddSignInManager()
           .AddDefaultTokenProviders();

            return builder;
        }
    }
}
