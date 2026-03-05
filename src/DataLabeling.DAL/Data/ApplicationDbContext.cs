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
        public DbSet<Label> Labels { get; set; }
        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<DatasetRound> DatasetRounds { get; set; }
        public DbSet<DataItem> DataItems { get; set; }
        public DbSet<Entities.Task> Tasks{ get; set; }

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

            // ======================
            // LABEL
            // ======================
            modelBuilder.Entity<Label>(entity =>
            {
                entity.ToTable("labels");

                entity.HasKey(e => e.LabelId);

                entity.Property(e => e.LabelId)
                      .HasColumnName("label_id");

                entity.Property(e => e.ProjectId)
                      .HasColumnName("project_id");

                entity.Property(e => e.LabelName)
                      .HasColumnName("label_name")
                      .HasMaxLength(255);

                entity.Property(e => e.LabelType)
                      .HasColumnName("label_type")
                      .HasMaxLength(100);

                entity.Property(e => e.Description)
                      .HasColumnName("description");

                entity.HasOne(e => e.Project)
                      .WithMany(p => p.Labels)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================
            // DATASET
            // ======================
            modelBuilder.Entity<Dataset>(entity =>
            {
                entity.ToTable("datasets");

                entity.HasKey(e => e.DatasetId);

                entity.Property(e => e.DatasetId)
                      .HasColumnName("dataset_id");

                entity.Property(e => e.ProjectId)
                      .HasColumnName("project_id");

                entity.Property(e => e.DatasetName)
                      .HasColumnName("dataset_name")
                      .HasMaxLength(255);

                entity.Property(e => e.Status)
                      .HasColumnName("status")
                      .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                      .HasColumnName("created_at");

                entity.HasOne(e => e.Project)
                      .WithMany(p => p.Datasets)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================
            // DATASET ROUND
            // ======================
            modelBuilder.Entity<DatasetRound>(entity =>
            {
                entity.ToTable("dataset_rounds");

                entity.HasKey(e => e.DatasetRoundId);

                entity.Property(e => e.DatasetRoundId)
                      .HasColumnName("dataset_round_id");

                entity.Property(e => e.DatasetId)
                      .HasColumnName("dataset_id");

                entity.Property(e => e.RoundId)
                      .HasColumnName("round_id");

                modelBuilder.Entity<DatasetRound>()
                        .Property(e => e.Status)
                        .HasConversion<string>()
                        .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                      .HasColumnName("created_at");

                entity.Property(e => e.CompletedAt)
                      .HasColumnName("completed_at");

                entity.HasOne(e => e.Dataset)
                      .WithMany(d => d.DatasetRounds)
                      .HasForeignKey(e => e.DatasetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DataItem>(entity =>
            {
                entity.ToTable("data_items");

                entity.HasKey(e => e.ItemId);

                entity.Property(e => e.ItemId)
                    .HasColumnName("item_id");

                entity.Property(e => e.DatasetId)
                    .HasColumnName("dataset_id");

                entity.Property(e => e.FileUrl)
                    .HasColumnName("file_url")
                    .HasMaxLength(500);

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasColumnName("status");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.HasOne(e => e.Dataset)
                    .WithMany(d => d.DataItems)
                    .HasForeignKey(e => e.DatasetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Entities.Task>(entity =>
            {
                entity.ToTable("tasks");

                entity.HasKey(e => e.TaskId);

                entity.Property(e => e.TaskId)
                    .HasColumnName("task_id");

                entity.Property(e => e.DatasetRoundId)
                    .HasColumnName("dataset_round_id");

                entity.Property(e => e.AssigneeUserId)
                    .HasColumnName("assignee_user_id");

                entity.Property(e => e.Type)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasColumnName("type");

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasColumnName("status");

                entity.Property(e => e.GroupNumber)
                    .HasColumnName("group_number");

                entity.Property(e => e.ParentTaskId)
                    .HasColumnName("parent_task_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.CompletedAt)
                    .HasColumnName("completed_at");

                // Relation: Task -> DatasetRound
                entity.HasOne(e => e.DatasetRound)
                    .WithMany(d => d.Tasks)
                    .HasForeignKey(e => e.DatasetRoundId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relation: Task -> User
                entity.HasOne(e => e.AssigneeUser)
                    .WithMany(u => u.AssignedTasks)
                    .HasForeignKey(e => e.AssigneeUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Self reference (Parent Task)
                entity.HasOne(e => e.ParentTask)
                    .WithMany(e => e.SubTasks)
                    .HasForeignKey(e => e.ParentTaskId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
