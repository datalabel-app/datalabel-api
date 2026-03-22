using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLabeling.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixEntityFlowv4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_annotations_tasks_TaskId",
                table: "annotations");

            migrationBuilder.DropForeignKey(
                name: "FK_annotations_users_AnnotatorId",
                table: "annotations");

            migrationBuilder.DropForeignKey(
                name: "fk_task_error_histories_data_items",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "fk_task_error_histories_tasks",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "fk_task_error_histories_users",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_data_items_data_item_id",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_data_item_id",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "data_item_id",
                table: "tasks");

            migrationBuilder.RenameColumn(
                name: "Classification",
                table: "annotations",
                newName: "classification");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "annotations",
                newName: "task_id");

            migrationBuilder.RenameColumn(
                name: "AnnotatorId",
                table: "annotations",
                newName: "annotator_id");

            migrationBuilder.RenameIndex(
                name: "IX_annotations_TaskId",
                table: "annotations",
                newName: "IX_annotations_task_id");

            migrationBuilder.RenameIndex(
                name: "IX_annotations_AnnotatorId",
                table: "annotations",
                newName: "IX_annotations_annotator_id");

            migrationBuilder.AddColumn<int>(
                name: "parent_dataset_id",
                table: "datasets",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "task_data_items",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    DataItemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_data_items", x => new { x.TaskId, x.DataItemId });
                    table.ForeignKey(
                        name: "FK_task_data_items_data_items_DataItemId",
                        column: x => x.DataItemId,
                        principalTable: "data_items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_data_items_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_datasets_parent_dataset_id",
                table: "datasets",
                column: "parent_dataset_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_data_items_DataItemId",
                table: "task_data_items",
                column: "DataItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_annotations_tasks_task_id",
                table: "annotations",
                column: "task_id",
                principalTable: "tasks",
                principalColumn: "task_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_annotations_users_annotator_id",
                table: "annotations",
                column: "annotator_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_datasets_datasets_parent_dataset_id",
                table: "datasets",
                column: "parent_dataset_id",
                principalTable: "datasets",
                principalColumn: "dataset_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_task_error_histories_data_items_item_id",
                table: "task_error_histories",
                column: "item_id",
                principalTable: "data_items",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_task_error_histories_tasks_task_id",
                table: "task_error_histories",
                column: "task_id",
                principalTable: "tasks",
                principalColumn: "task_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_task_error_histories_users_reviewer_id",
                table: "task_error_histories",
                column: "reviewer_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_annotations_tasks_task_id",
                table: "annotations");

            migrationBuilder.DropForeignKey(
                name: "FK_annotations_users_annotator_id",
                table: "annotations");

            migrationBuilder.DropForeignKey(
                name: "FK_datasets_datasets_parent_dataset_id",
                table: "datasets");

            migrationBuilder.DropForeignKey(
                name: "FK_task_error_histories_data_items_item_id",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "FK_task_error_histories_tasks_task_id",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "FK_task_error_histories_users_reviewer_id",
                table: "task_error_histories");

            migrationBuilder.DropTable(
                name: "task_data_items");

            migrationBuilder.DropIndex(
                name: "IX_datasets_parent_dataset_id",
                table: "datasets");

            migrationBuilder.DropColumn(
                name: "parent_dataset_id",
                table: "datasets");

            migrationBuilder.RenameColumn(
                name: "classification",
                table: "annotations",
                newName: "Classification");

            migrationBuilder.RenameColumn(
                name: "task_id",
                table: "annotations",
                newName: "TaskId");

            migrationBuilder.RenameColumn(
                name: "annotator_id",
                table: "annotations",
                newName: "AnnotatorId");

            migrationBuilder.RenameIndex(
                name: "IX_annotations_task_id",
                table: "annotations",
                newName: "IX_annotations_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_annotations_annotator_id",
                table: "annotations",
                newName: "IX_annotations_AnnotatorId");

            migrationBuilder.AddColumn<int>(
                name: "data_item_id",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_tasks_data_item_id",
                table: "tasks",
                column: "data_item_id");

            migrationBuilder.AddForeignKey(
                name: "FK_annotations_tasks_TaskId",
                table: "annotations",
                column: "TaskId",
                principalTable: "tasks",
                principalColumn: "task_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_annotations_users_AnnotatorId",
                table: "annotations",
                column: "AnnotatorId",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_task_error_histories_data_items",
                table: "task_error_histories",
                column: "item_id",
                principalTable: "data_items",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_task_error_histories_tasks",
                table: "task_error_histories",
                column: "task_id",
                principalTable: "tasks",
                principalColumn: "task_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_task_error_histories_users",
                table: "task_error_histories",
                column: "reviewer_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_data_items_data_item_id",
                table: "tasks",
                column: "data_item_id",
                principalTable: "data_items",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
