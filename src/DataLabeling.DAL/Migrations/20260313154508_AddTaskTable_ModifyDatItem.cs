using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLabeling.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskTable_ModifyDatItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_data_items_users_annotator_id",
                table: "data_items");

            migrationBuilder.DropForeignKey(
                name: "FK_data_items_users_reviewer_id",
                table: "data_items");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Task_ParentTaskId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_dataset_rounds_DatasetRoundId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_users_AssigneeUserId",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_data_items_annotator_id",
                table: "data_items");

            migrationBuilder.DropIndex(
                name: "IX_data_items_reviewer_id",
                table: "data_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Task",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_AssigneeUserId",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_DatasetRoundId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "annotator_id",
                table: "data_items");

            migrationBuilder.DropColumn(
                name: "reviewer_id",
                table: "data_items");

            migrationBuilder.DropColumn(
                name: "AssigneeUserId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "DatasetRoundId",
                table: "Task");

            migrationBuilder.RenameTable(
                name: "Task",
                newName: "tasks");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "tasks",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "tasks",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "tasks",
                newName: "task_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "tasks",
                newName: "round_id");

            migrationBuilder.RenameColumn(
                name: "ParentTaskId",
                table: "tasks",
                newName: "reviewer_id");

            migrationBuilder.RenameColumn(
                name: "GroupNumber",
                table: "tasks",
                newName: "data_item_id");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "tasks",
                newName: "reviewed_at");

            migrationBuilder.RenameIndex(
                name: "IX_Task_ParentTaskId",
                table: "tasks",
                newName: "IX_tasks_reviewer_id");

            migrationBuilder.AddColumn<int>(
                name: "ShapeType",
                table: "dataset_rounds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "annotations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "annotated_at",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "annotator_id",
                table: "tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_tasks",
                table: "tasks",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_annotations_TaskId",
                table: "annotations",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_annotator_id",
                table: "tasks",
                column: "annotator_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_data_item_id",
                table: "tasks",
                column: "data_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_round_id",
                table: "tasks",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_UserId",
                table: "tasks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_annotations_tasks_TaskId",
                table: "annotations",
                column: "TaskId",
                principalTable: "tasks",
                principalColumn: "task_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_data_items_data_item_id",
                table: "tasks",
                column: "data_item_id",
                principalTable: "data_items",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_dataset_rounds_round_id",
                table: "tasks",
                column: "round_id",
                principalTable: "dataset_rounds",
                principalColumn: "round_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_users_UserId",
                table: "tasks",
                column: "UserId",
                principalTable: "users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_users_annotator_id",
                table: "tasks",
                column: "annotator_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_users_reviewer_id",
                table: "tasks",
                column: "reviewer_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_annotations_tasks_TaskId",
                table: "annotations");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_data_items_data_item_id",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_dataset_rounds_round_id",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_users_UserId",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_users_annotator_id",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_users_reviewer_id",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_annotations_TaskId",
                table: "annotations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tasks",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_annotator_id",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_data_item_id",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_round_id",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_UserId",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "ShapeType",
                table: "dataset_rounds");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "annotations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "annotated_at",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "annotator_id",
                table: "tasks");

            migrationBuilder.RenameTable(
                name: "tasks",
                newName: "Task");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Task",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Task",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "task_id",
                table: "Task",
                newName: "TaskId");

            migrationBuilder.RenameColumn(
                name: "round_id",
                table: "Task",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "reviewer_id",
                table: "Task",
                newName: "ParentTaskId");

            migrationBuilder.RenameColumn(
                name: "reviewed_at",
                table: "Task",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "data_item_id",
                table: "Task",
                newName: "GroupNumber");

            migrationBuilder.RenameIndex(
                name: "IX_tasks_reviewer_id",
                table: "Task",
                newName: "IX_Task_ParentTaskId");

            migrationBuilder.AddColumn<int>(
                name: "annotator_id",
                table: "data_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reviewer_id",
                table: "data_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssigneeUserId",
                table: "Task",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DatasetRoundId",
                table: "Task",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Task",
                table: "Task",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_data_items_annotator_id",
                table: "data_items",
                column: "annotator_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_items_reviewer_id",
                table: "data_items",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_AssigneeUserId",
                table: "Task",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Task_DatasetRoundId",
                table: "Task",
                column: "DatasetRoundId");

            migrationBuilder.AddForeignKey(
                name: "FK_data_items_users_annotator_id",
                table: "data_items",
                column: "annotator_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_data_items_users_reviewer_id",
                table: "data_items",
                column: "reviewer_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Task_ParentTaskId",
                table: "Task",
                column: "ParentTaskId",
                principalTable: "Task",
                principalColumn: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_dataset_rounds_DatasetRoundId",
                table: "Task",
                column: "DatasetRoundId",
                principalTable: "dataset_rounds",
                principalColumn: "round_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_users_AssigneeUserId",
                table: "Task",
                column: "AssigneeUserId",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
