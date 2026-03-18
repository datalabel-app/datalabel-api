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
        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<DatasetRound> DatasetRounds { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<DataItem> DataItems { get; set; }
        public DbSet<Annotation> Annotations { get; set; }
        public DbSet<Entities.Task> Tasks { get; set; }

        public DbSet<Token> Token { get; set; }

        public DbSet<TaskErrorHistory> TaskErrorHistories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================
            // USER
            // ======================
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
                entity.Property(e => e.Password).HasColumnName("password");
                entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasMany(e => e.Projects)
                      .WithOne(p => p.Manager)
                      .HasForeignKey(p => p.ManagerId)
                      .OnDelete(DeleteBehavior.Cascade);

            });

            // ======================
            // PROJECT
            // ======================
            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("projects");

                entity.HasKey(e => e.ProjectId);

                entity.Property(e => e.ProjectId).HasColumnName("project_id");
                entity.Property(e => e.ManagerId).HasColumnName("manager_id");
                entity.Property(e => e.ProjectName).HasColumnName("project_name").HasMaxLength(255);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            // ======================
            // DATASET
            // ======================
            modelBuilder.Entity<Dataset>(entity =>
            {
                entity.ToTable("datasets");

                entity.HasKey(e => e.DatasetId);

                entity.Property(e => e.DatasetId).HasColumnName("dataset_id");
                entity.Property(e => e.ProjectId).HasColumnName("project_id");
                entity.Property(e => e.DatasetName).HasColumnName("dataset_name").HasMaxLength(255);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

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

                entity.HasKey(e => e.RoundId);

                entity.Property(e => e.RoundId).HasColumnName("round_id");
                entity.Property(e => e.DatasetId).HasColumnName("dataset_id");
                entity.Property(e => e.RoundNumber).HasColumnName("round_number");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(e => e.Dataset)
                      .WithMany(d => d.Rounds)
                      .HasForeignKey(e => e.DatasetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================
            // LABEL
            // ======================
            modelBuilder.Entity<Label>(entity =>
            {
                entity.ToTable("labels");

                entity.HasKey(e => e.LabelId);

                entity.Property(e => e.LabelId).HasColumnName("label_id");
                entity.Property(e => e.RoundId).HasColumnName("round_id");
                entity.Property(e => e.LabelName).HasColumnName("label_name").HasMaxLength(255);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.LabelStatus).HasColumnName("label_status").HasMaxLength(50);

                entity.Property(e => e.AnnotatorId).HasColumnName("annotator_id");

                entity.HasOne(e => e.Annotator)
                      .WithMany()
                      .HasForeignKey(e => e.AnnotatorId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Round)
                      .WithMany(r => r.Labels)
                      .HasForeignKey(e => e.RoundId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================
            // DATA ITEM
            // ======================
            modelBuilder.Entity<DataItem>(entity =>
            {
                entity.ToTable("data_items");

                entity.HasKey(e => e.ItemId);

                entity.Property(e => e.ItemId).HasColumnName("item_id");
                entity.Property(e => e.DatasetId).HasColumnName("dataset_id");
                entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(500);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");


            });

            // ======================
            // ANNOTATION
            // ======================
            modelBuilder.Entity<Annotation>(entity =>
            {
                entity.ToTable("annotations");

                entity.HasKey(e => e.AnnotationId);

                entity.Property(e => e.AnnotationId).HasColumnName("annotation_id");
                entity.Property(e => e.ItemId).HasColumnName("item_id");
                entity.Property(e => e.LabelId).HasColumnName("label_id");
                entity.Property(e => e.ShapeType).HasColumnName("shape_type");
                entity.Property(e => e.Coordinates).HasColumnName("coordinates");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(e => e.DataItem)
                      .WithMany(d => d.Annotations)
                      .HasForeignKey(e => e.ItemId);

                entity.HasOne(e => e.Label)
                      .WithMany()
                      .HasForeignKey(e => e.LabelId);
            });

            modelBuilder.Entity<Entities.Task>(entity =>
            {
                entity.ToTable("tasks");

                entity.HasKey(e => e.TaskId);

                entity.Property(e => e.TaskId).HasColumnName("task_id");
                entity.Property(e => e.DataItemId).HasColumnName("data_item_id");
                entity.Property(e => e.RoundId).HasColumnName("round_id");
                entity.Property(e => e.AnnotatorId).HasColumnName("annotator_id");
                entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.AnnotatedAt).HasColumnName("annotated_at");
                entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
                entity.Property(e => e.DescriptionError).HasColumnName("description_error");
                entity.HasOne(e => e.DataItem)
                      .WithMany(d => d.Tasks)
                      .HasForeignKey(e => e.DataItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Round)
                      .WithMany(r => r.Tasks)
                      .HasForeignKey(e => e.RoundId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Annotator)
                      .WithMany()
                      .HasForeignKey(e => e.AnnotatorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Reviewer)
                      .WithMany()
                      .HasForeignKey(e => e.ReviewerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.ToTable("tokens");

                entity.HasKey(e => e.TokenId);

                entity.Property(e => e.TokenId).HasColumnName("token_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.TokenType).HasColumnName("token_type");
                entity.Property(e => e.TokenValue).HasColumnName("token_value");
                entity.Property(e => e.Expired).HasColumnName("expired");
                entity.Property(e => e.IsUsed).HasColumnName("is_used");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");


            });


            modelBuilder.Entity<TaskErrorHistory>(entity =>
            {
                entity.ToTable("task_error_histories");

                entity.HasKey(e => e.ErrorId);

                entity.Property(e => e.ErrorId)
                    .HasColumnName("error_id");

                entity.Property(e => e.TaskId)
                    .HasColumnName("task_id");

                entity.Property(e => e.ItemId)
                    .HasColumnName("item_id");

                entity.Property(e => e.ReviewerId)
                    .HasColumnName("reviewer_id");

                entity.Property(e => e.ErrorMessage)
                    .HasColumnName("error_message")
                    .HasMaxLength(1000);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.HasOne(e => e.DataItem)
                    .WithMany()
                    .HasForeignKey(e => e.ItemId)
                    .HasConstraintName("fk_task_error_histories_data_items");

                entity.HasOne(e => e.Task)
                    .WithMany(t => t.ErrorHistories)
                    .HasForeignKey(e => e.TaskId)
                    .HasConstraintName("fk_task_error_histories_tasks");

                entity.HasOne(e => e.Reviewer)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewerId)
                    .HasConstraintName("fk_task_error_histories_users");
            });

        }
    }
}
