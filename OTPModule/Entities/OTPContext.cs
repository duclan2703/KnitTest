using Microsoft.EntityFrameworkCore;

namespace OTPModule.Entities
{
    public class OTPContext : DbContext
    {
        public OTPContext(DbContextOptions<OTPContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder) { }
        public DbSet<OTPCheck> OTPChecks { get; set; }
    }
}
