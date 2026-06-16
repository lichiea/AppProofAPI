// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using ProofAPI.Models;

namespace ProofAPI.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<TestRun> TestRuns { get; set; }
        public DbSet<Vulnerability> Vulnerabilities { get; set; }
        public DbSet<SecurityHeadersResult> SecurityHeadersResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Файл БД будет создан в папке приложения
            optionsBuilder.UseSqlite("Data Source=proofapi.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка внешних ключей и каскадного удаления
            modelBuilder.Entity<TestRun>()
                .HasOne(t => t.Project)
                .WithMany(p => p.TestRuns)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vulnerability>()
                .HasOne(v => v.TestRun)
                .WithMany(t => t.Vulnerabilities)
                .HasForeignKey(v => v.TestRunId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SecurityHeadersResult>()
                .HasOne(s => s.TestRun)
                .WithMany(t => t.SecurityHeadersResults)
                .HasForeignKey(s => s.TestRunId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}