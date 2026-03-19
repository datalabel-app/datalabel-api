using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLabeling.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldEntityLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "label_status",
                table: "labels",
                type: "integer",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "label_status",
                table: "labels");
        }
    }
}
