using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLabeling.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SomeThingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_annotations_tasks_TaskId",
                table: "annotations");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "annotations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_annotations_tasks_TaskId",
                table: "annotations",
                column: "TaskId",
                principalTable: "tasks",
                principalColumn: "task_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_annotations_tasks_TaskId",
                table: "annotations");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "annotations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_annotations_tasks_TaskId",
                table: "annotations",
                column: "TaskId",
                principalTable: "tasks",
                principalColumn: "task_id");
        }
    }
}
