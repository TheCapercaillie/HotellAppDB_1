﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotellAppDB.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsAvailableFromRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Rooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Rooms",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
