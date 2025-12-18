using Emustates.Site.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Emustates.Infra.Data
{
    public static class DependencyInjectionExtensions
    {
        public static TBuilder AddDataEntityFrameworkCore<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<EmustatesDbContext>(options =>
                options.UseSqlServer(connectionString));

            return builder;
        }
    }
}
