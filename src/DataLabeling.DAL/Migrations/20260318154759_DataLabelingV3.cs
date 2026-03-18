using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataLabeling.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DataLabelingV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description_error",
                table: "tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "labels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "annotator_id",
                table: "labels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "label_status",
                table: "labels",
                type: "integer",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);

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
                    DataItemItemId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_error_histories", x => x.error_id);
                    table.ForeignKey(
                        name: "FK_task_error_histories_data_items_DataItemItemId",
                        column: x => x.DataItemItemId,
                        principalTable: "data_items",
                        principalColumn: "item_id");
                    table.ForeignKey(
                        name: "FK_task_error_histories_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "fk_task_error_histories_data_items",
                        column: x => x.item_id,
                        principalTable: "data_items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_error_histories_tasks",
                        column: x => x.task_id,
                        principalTable: "tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_error_histories_users",
                        column: x => x.reviewer_id,
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

            migrationBuilder.CreateIndex(
                name: "IX_labels_annotator_id",
                table: "labels",
                column: "annotator_id");

            migrationBuilder.CreateIndex(
                name: "IX_labels_UserId",
                table: "labels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_task_error_histories_DataItemItemId",
                table: "task_error_histories",
                column: "DataItemItemId");

            migrationBuilder.CreateIndex(
                name: "IX_task_error_histories_item_id",
                table: "task_error_histories",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_error_histories_reviewer_id",
                table: "task_error_histories",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_error_histories_task_id",
                table: "task_error_histories",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_error_histories_UserId",
                table: "task_error_histories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_user_id",
                table: "tokens",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_labels_users_UserId",
                table: "labels",
                column: "UserId",
                principalTable: "users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_labels_users_annotator_id",
                table: "labels",
                column: "annotator_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_labels_users_UserId",
                table: "labels");

            migrationBuilder.DropForeignKey(
                name: "FK_labels_users_annotator_id",
                table: "labels");

            migrationBuilder.DropTable(
                name: "task_error_histories");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropIndex(
                name: "IX_labels_annotator_id",
                table: "labels");

            migrationBuilder.DropIndex(
                name: "IX_labels_UserId",
                table: "labels");

            migrationBuilder.DropColumn(
                name: "description_error",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "labels");

            migrationBuilder.DropColumn(
                name: "annotator_id",
                table: "labels");

            migrationBuilder.DropColumn(
                name: "label_status",
                table: "labels");
        }
    }
}
