using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLabeling.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixEntityDataset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "label_id",
                table: "datasets",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_datasets_label_id",
                table: "datasets",
                column: "label_id");

            migrationBuilder.AddForeignKey(
                name: "FK_datasets_labels_label_id",
                table: "datasets",
                column: "label_id",
                principalTable: "labels",
                principalColumn: "label_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_datasets_labels_label_id",
                table: "datasets");

            migrationBuilder.DropIndex(
                name: "IX_datasets_label_id",
                table: "datasets");

            migrationBuilder.DropColumn(
                name: "label_id",
                table: "datasets");
        }
    }
}
