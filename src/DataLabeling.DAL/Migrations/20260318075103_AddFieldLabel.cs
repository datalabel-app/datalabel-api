using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLabeling.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_labels_annotator_id",
                table: "labels",
                column: "annotator_id");

            migrationBuilder.CreateIndex(
                name: "IX_labels_UserId",
                table: "labels",
                column: "UserId");

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

            migrationBuilder.DropIndex(
                name: "IX_labels_annotator_id",
                table: "labels");

            migrationBuilder.DropIndex(
                name: "IX_labels_UserId",
                table: "labels");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "labels");

            migrationBuilder.DropColumn(
                name: "annotator_id",
                table: "labels");
        }
    }
}
