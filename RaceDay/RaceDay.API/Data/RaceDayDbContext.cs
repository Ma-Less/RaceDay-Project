using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<EventImage> EventImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraint on Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Unique constraint on Enrolment (one participant per event)
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ParticipantUserID, e.EventID })
                .IsUnique();

            // Unique constraint on Result (one result per enrolment)
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentID)
                .IsUnique();

            // One-to-One: Enrolment -> Result
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Result)
                .WithOne(r => r.Enrolment)
                .HasForeignKey<Result>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent cascade delete loops
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.ParticipantUser)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(e => e.ParticipantUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(ev => ev.CreatedByUser)
                .WithMany(u => u.CreatedEvents)
                .HasForeignKey(ev => ev.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}