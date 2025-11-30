using Emustates.Site.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Emustates.Infra.Data
{
    public class EmustatesDbContext(DbContextOptions<EmustatesDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
    }
}
