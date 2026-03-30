using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLabeling.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_datasets_datasets_parent_dataset_id",
                table: "datasets");

            migrationBuilder.AddForeignKey(
                name: "FK_datasets_datasets_parent_dataset_id",
                table: "datasets",
                column: "parent_dataset_id",
                principalTable: "datasets",
                principalColumn: "dataset_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_datasets_datasets_parent_dataset_id",
                table: "datasets");

            migrationBuilder.AddForeignKey(
                name: "FK_datasets_datasets_parent_dataset_id",
                table: "datasets",
                column: "parent_dataset_id",
                principalTable: "datasets",
                principalColumn: "dataset_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
