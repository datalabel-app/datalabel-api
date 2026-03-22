using DataLabeling.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataLabeling.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<DatasetRound> DatasetRounds { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<DataItem> DataItems { get; set; }
        public DbSet<Annotation> Annotations { get; set; }
        public DbSet<DataLabeling.Entities.Task> Tasks { get; set; }
        public DbSet<TaskDataItem> TaskDataItems { get; set; }

        public DbSet<Token> Token { get; set; }
        public DbSet<TaskErrorHistory> TaskErrorHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------
            // USER
            // -----------------------
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

            // -----------------------
            // PROJECT
            // -----------------------
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

            // -----------------------
            // DATASET
            // -----------------------
            modelBuilder.Entity<Dataset>(entity =>
            {
                entity.ToTable("datasets");
                entity.HasKey(e => e.DatasetId);
                entity.Property(e => e.DatasetId).HasColumnName("dataset_id");
                entity.Property(e => e.ParentDatasetId).HasColumnName("parent_dataset_id");
                entity.Property(e => e.ProjectId).HasColumnName("project_id");
                entity.Property(e => e.DatasetName).HasColumnName("dataset_name").HasMaxLength(255);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(e => e.Project)
                      .WithMany(p => p.Datasets)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ParentDataset)
                      .WithMany(d => d.SubDatasets)
                      .HasForeignKey(e => e.ParentDatasetId)
                      .OnDelete(DeleteBehavior.Restrict); // giữ subdataset nếu cha xóa
            });

            // -----------------------
            // DATASET ROUND
            // -----------------------
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

            // -----------------------
            // LABEL
            // -----------------------
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

            // -----------------------
            // DATA ITEM
            // -----------------------
            modelBuilder.Entity<DataItem>(entity =>
            {
                entity.ToTable("data_items");
                entity.HasKey(e => e.ItemId);
                entity.Property(e => e.ItemId).HasColumnName("item_id");
                entity.Property(e => e.DatasetId).HasColumnName("dataset_id");
                entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(500);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(d => d.Dataset)
                      .WithMany(ds => ds.DataItems)
                      .HasForeignKey(d => d.DatasetId)
                      .OnDelete(DeleteBehavior.Cascade);


            });

            // -----------------------
            // TASK
            // -----------------------
            modelBuilder.Entity<DataLabeling.Entities.Task>(entity =>
            {
                entity.ToTable("tasks");
                entity.HasKey(e => e.TaskId);
                entity.Property(e => e.TaskId).HasColumnName("task_id");
                entity.Property(e => e.RoundId).HasColumnName("round_id");
                entity.Property(e => e.AnnotatorId).HasColumnName("annotator_id");
                entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.AnnotatedAt).HasColumnName("annotated_at");
                entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
                entity.Property(e => e.DescriptionError).HasColumnName("description_error");

                entity.HasOne(t => t.Round)
                      .WithMany(r => r.Tasks)
                      .HasForeignKey(t => t.RoundId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Annotator)
                      .WithMany()
                      .HasForeignKey(t => t.AnnotatorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Reviewer)
                      .WithMany()
                      .HasForeignKey(t => t.ReviewerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // -----------------------
            // TASK-DATAITEM (many-to-many)
            // -----------------------
            modelBuilder.Entity<TaskDataItem>(entity =>
            {
                entity.ToTable("task_data_items");

                entity.HasKey(td => new { td.TaskId, td.DataItemId });

                entity.HasOne(td => td.Task)
                      .WithMany(t => t.TaskDataItems)
                      .HasForeignKey(td => td.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(td => td.DataItem)
                      .WithMany(d => d.TaskDataItems)
                      .HasForeignKey(td => td.DataItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(td => td.ReviewStatus)
                      .HasMaxLength(20)
                      .HasDefaultValue("Pending");

                entity.Property(td => td.ReviewComment)
                      .HasMaxLength(500);

                entity.Property(td => td.ReviewerId)
                      .IsRequired(false);

                entity.Property(td => td.ReviewedAt)
                      .IsRequired(false);
            });

            // -----------------------
            // ANNOTATION
            // -----------------------
            modelBuilder.Entity<Annotation>(entity =>
            {
                entity.ToTable("annotations");
                entity.HasKey(a => a.AnnotationId);
                entity.Property(a => a.AnnotationId).HasColumnName("annotation_id");
                entity.Property(a => a.ItemId).HasColumnName("item_id");
                entity.Property(a => a.LabelId).HasColumnName("label_id");
                entity.Property(a => a.TaskId).HasColumnName("task_id");
                entity.Property(a => a.AnnotatorId).HasColumnName("annotator_id");
                entity.Property(a => a.ShapeType).HasColumnName("shape_type");
                entity.Property(a => a.Coordinates).HasColumnName("coordinates");
                entity.Property(a => a.Classification).HasColumnName("classification");
                entity.Property(a => a.CreatedAt).HasColumnName("created_at");

                entity.HasOne(a => a.DataItem)
                      .WithMany(d => d.Annotations)
                      .HasForeignKey(a => a.ItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Label)
                      .WithMany()
                      .HasForeignKey(a => a.LabelId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Task)
                      .WithMany(t => t.Annotations)
                      .HasForeignKey(a => a.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Annotator)
                      .WithMany()
                      .HasForeignKey(a => a.AnnotatorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // -----------------------
            // TASK ERROR HISTORY
            // -----------------------
            modelBuilder.Entity<TaskErrorHistory>(entity =>
            {
                entity.ToTable("task_error_histories");
                entity.HasKey(e => e.ErrorId);

                entity.Property(e => e.ErrorId).HasColumnName("error_id");
                entity.Property(e => e.TaskId).HasColumnName("task_id");
                entity.Property(e => e.ItemId).HasColumnName("item_id");
                entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
                entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(e => e.Task)
                      .WithMany(t => t.ErrorHistories)
                      .HasForeignKey(e => e.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.DataItem)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Reviewer)
                      .WithMany()
                      .HasForeignKey(e => e.ReviewerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // -----------------------
            // TOKEN
            // -----------------------
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
        }
    }
}
