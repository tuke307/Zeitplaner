using Microsoft.EntityFrameworkCore;
using System;
using ZeitPlaner.Data.Models;

namespace ZeitPlaner.Data
{
    public class ZeitplanerDataContext : DbContext
    {
        public DbSet<Kunde> Kunde { get; set; }
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
