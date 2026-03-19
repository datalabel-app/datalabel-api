using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLabeling.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixTaskErrorHistoryRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_task_error_histories_data_items_DataItemItemId",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "FK_task_error_histories_tasks_task_id",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "FK_task_error_histories_users_reviewer_id",
                table: "task_error_histories");

            migrationBuilder.AlterColumn<int>(
                name: "DataItemItemId",
                table: "task_error_histories",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "task_error_histories",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_task_error_histories_item_id",
                table: "task_error_histories",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_error_histories_UserId",
                table: "task_error_histories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_task_error_histories_data_items_DataItemItemId",
                table: "task_error_histories",
                column: "DataItemItemId",
                principalTable: "data_items",
                principalColumn: "item_id");

            migrationBuilder.AddForeignKey(
                name: "FK_task_error_histories_users_UserId",
                table: "task_error_histories",
                column: "UserId",
                principalTable: "users",
                principalColumn: "user_id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_task_error_histories_data_items_DataItemItemId",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "FK_task_error_histories_users_UserId",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "fk_task_error_histories_data_items",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "fk_task_error_histories_tasks",
                table: "task_error_histories");

            migrationBuilder.DropForeignKey(
                name: "fk_task_error_histories_users",
                table: "task_error_histories");

            migrationBuilder.DropIndex(
                name: "IX_task_error_histories_item_id",
                table: "task_error_histories");

            migrationBuilder.DropIndex(
                name: "IX_task_error_histories_UserId",
                table: "task_error_histories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "task_error_histories");

            migrationBuilder.AlterColumn<int>(
                name: "DataItemItemId",
                table: "task_error_histories",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_task_error_histories_data_items_DataItemItemId",
                table: "task_error_histories",
                column: "DataItemItemId",
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
                onDelete: ReferentialAction.Cascade);
        }
    }
}
