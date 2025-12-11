using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdAnimal.API.Migrations
{
    /// <inheritdoc />
    public partial class SyncFixesOrPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                table: "Cattle",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Cattle");
        }
    }
}
