using DataLabeling.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                      .HasColumnName("user_id");

                entity.Property(e => e.FullName)
                      .HasColumnName("full_name")
                      .HasMaxLength(255);

                entity.Property(e => e.Email)
                      .HasColumnName("email")
                      .HasMaxLength(255);

                entity.Property(e => e.Password)
                      .HasColumnName("password");

                entity.Property(e => e.Role)
                      .HasColumnName("role")
                      .HasMaxLength(50);

                entity.Property(e => e.Status)
                      .HasColumnName("status")
                      .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                      .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                      .HasColumnName("updated_at");

                // Quan hệ 1 User - N Project
                entity.HasMany(e => e.Projects)
                      .WithOne(p => p.Manager)
                      .HasForeignKey(p => p.ManagerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("projects");

                entity.HasKey(e => e.ProjectId);

                entity.Property(e => e.ProjectId)
                      .HasColumnName("project_id");

                entity.Property(e => e.ManagerId)
                      .HasColumnName("manager_id");

                entity.Property(e => e.ProjectName)
                      .HasColumnName("project_name")
                      .HasMaxLength(255);

                entity.Property(e => e.Description)
                      .HasColumnName("description");

                entity.Property(e => e.Status)
                      .HasColumnName("status")
                      .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                      .HasColumnName("created_at");
            });
        }
    }
}
