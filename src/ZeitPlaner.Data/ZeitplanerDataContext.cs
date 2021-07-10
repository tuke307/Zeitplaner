using Microsoft.EntityFrameworkCore;
using ZeitPlaner.Data.Models;

namespace ZeitPlaner.Data
{
    /// <summary>
    /// ZeitplanerDataContext.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public class ZeitplanerDataContext : DbContext
    {
        /// <summary>
        /// Gets or sets the kunde.
        /// </summary>
        public DbSet<Kunde> Kunde { get; set; }

        /// <summary>
        /// Gets or sets the bemerkung.
        /// </summary>
        /// <value>
        public DbSet<Bemerkung> Bemerkung { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlite(@"Filename=C:\Users\praktikant\Desktop\zeitplaner.db");

            optionsBuilder.UseSqlite("Filename=" + Constants.DatabaseFilePath);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kunde>()
                .HasMany(i => i.Bemerkungen)
                .WithOne(i => i.Kunde)
                .HasForeignKey(i => i.KundeID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}