using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotellAppDB.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraBedsToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExtraBeds",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraBeds",
                table: "Rooms");
        }
    }
}
