using Microsoft.EntityFrameworkCore;
using NetDevRecruitingTest.src.Domain;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NetDevRecruitingTest.src.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<VacationPackage> VacationPackages { get; set; }
        public DbSet<Vacation> Vacations { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Team)
                .WithMany(t => t.Employees)
                .HasForeignKey(e => e.TeamId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.VacationPackage)
                .WithMany()  // Brak nawigacji zwrotnej
                .HasForeignKey(e => e.VacationPackageId);

            modelBuilder.Entity<Vacation>()
                .HasOne(v => v.Employee)
                .WithMany(e => e.Vacations)
                .HasForeignKey(v => v.EmployeeId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Superior)
                .WithMany()
                .HasForeignKey(e => e.SuperiorId)
                .OnDelete(DeleteBehavior.NoAction);  // Acyclic
        }
    }
}