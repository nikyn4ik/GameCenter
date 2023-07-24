using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GameCenter.Entities
{
    public class DBContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<IdentityUser> Users { get; set; }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }
    }
}