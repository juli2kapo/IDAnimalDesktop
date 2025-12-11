using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdAnimal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CattleVideos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CattleImages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CattleFullImages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Cattle",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CattleVideos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CattleImages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CattleFullImages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Cattle");
        }
    }
}
