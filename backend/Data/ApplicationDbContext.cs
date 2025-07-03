using backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace backend.Data
{
    public class ApplicationDbContext : IdentityDbContext 
    {
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<UserTask> UserTasks { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
