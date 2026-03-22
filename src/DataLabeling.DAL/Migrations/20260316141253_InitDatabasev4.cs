using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataLabeling.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabasev4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    manager_id = table.Column<int>(type: "integer", nullable: false),
                    project_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.project_id);
                    table.ForeignKey(
                        name: "FK_projects_users_manager_id",
                        column: x => x.manager_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    token_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    token_type = table.Column<string>(type: "text", nullable: false),
                    token_value = table.Column<string>(type: "text", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokens", x => x.token_id);
                    table.ForeignKey(
                        name: "FK_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "datasets",
                columns: table => new
                {
                    dataset_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    dataset_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_datasets", x => x.dataset_id);
                    table.ForeignKey(
                        name: "FK_datasets_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_items",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dataset_id = table.Column<int>(type: "integer", nullable: false),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_items", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_data_items_datasets_dataset_id",
                        column: x => x.dataset_id,
                        principalTable: "datasets",
                        principalColumn: "dataset_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dataset_rounds",
                columns: table => new
                {
                    round_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dataset_id = table.Column<int>(type: "integer", nullable: false),
                    round_number = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ShapeType = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dataset_rounds", x => x.round_id);
                    table.ForeignKey(
                        name: "FK_dataset_rounds_datasets_dataset_id",
                        column: x => x.dataset_id,
                        principalTable: "datasets",
                        principalColumn: "dataset_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "labels",
                columns: table => new
                {
                    label_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    round_id = table.Column<int>(type: "integer", nullable: false),
                    label_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_labels", x => x.label_id);
                    table.ForeignKey(
                        name: "FK_labels_dataset_rounds_round_id",
                        column: x => x.round_id,
                        principalTable: "dataset_rounds",
                        principalColumn: "round_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_item_id = table.Column<int>(type: "integer", nullable: false),
                    round_id = table.Column<int>(type: "integer", nullable: false),
                    annotator_id = table.Column<int>(type: "integer", nullable: true),
                    reviewer_id = table.Column<int>(type: "integer", nullable: true),
                    description_error = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    annotated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.task_id);
                    table.ForeignKey(
                        name: "FK_tasks_data_items_data_item_id",
                        column: x => x.data_item_id,
                        principalTable: "data_items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tasks_dataset_rounds_round_id",
                        column: x => x.round_id,
                        principalTable: "dataset_rounds",
                        principalColumn: "round_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tasks_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_tasks_users_annotator_id",
                        column: x => x.annotator_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tasks_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "annotations",
                columns: table => new
                {
                    annotation_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    label_id = table.Column<int>(type: "integer", nullable: false),
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    RoundId = table.Column<int>(type: "integer", nullable: false),
                    AnnotatorId = table.Column<int>(type: "integer", nullable: false),
                    shape_type = table.Column<string>(type: "text", nullable: false),
                    coordinates = table.Column<string>(type: "text", nullable: false),
                    Classification = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annotations", x => x.annotation_id);
                    table.ForeignKey(
                        name: "FK_annotations_data_items_item_id",
                        column: x => x.item_id,
                        principalTable: "data_items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_annotations_labels_label_id",
                        column: x => x.label_id,
                        principalTable: "labels",
                        principalColumn: "label_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_annotations_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_annotations_users_AnnotatorId",
                        column: x => x.AnnotatorId,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_error_histories",
                columns: table => new
                {
                    error_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    task_id = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    reviewer_id = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataItemItemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_error_histories", x => x.error_id);
                    table.ForeignKey(
                        name: "FK_task_error_histories_data_items_DataItemItemId",
                        column: x => x.DataItemItemId,
                        principalTable: "data_items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_error_histories_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_error_histories_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_annotations_AnnotatorId",
                table: "annotations",
                column: "AnnotatorId");

            migrationBuilder.CreateIndex(
                name: "IX_annotations_item_id",
                table: "annotations",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_annotations_label_id",
                table: "annotations",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "IX_annotations_TaskId",
                table: "annotations",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_data_items_dataset_id",
                table: "data_items",
                column: "dataset_id");

            migrationBuilder.CreateIndex(
                name: "IX_dataset_rounds_dataset_id",
                table: "dataset_rounds",
                column: "dataset_id");

            migrationBuilder.CreateIndex(
                name: "IX_datasets_project_id",
                table: "datasets",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_labels_round_id",
                table: "labels",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_manager_id",
                table: "projects",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_error_histories_DataItemItemId",
                table: "task_error_histories",
                column: "DataItemItemId");

            migrationBuilder.CreateIndex(
                name: "IX_task_error_histories_reviewer_id",
                table: "task_error_histories",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_error_histories_task_id",
                table: "task_error_histories",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_annotator_id",
                table: "tasks",
                column: "annotator_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_data_item_id",
                table: "tasks",
                column: "data_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_reviewer_id",
                table: "tasks",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_round_id",
                table: "tasks",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_UserId",
                table: "tasks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_user_id",
                table: "tokens",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "annotations");

            migrationBuilder.DropTable(
                name: "task_error_histories");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "labels");

            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "data_items");

            migrationBuilder.DropTable(
                name: "dataset_rounds");

            migrationBuilder.DropTable(
                name: "datasets");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
