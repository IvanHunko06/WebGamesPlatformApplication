using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamesService.Migrations
{
    /// <inheritdoc />
    public partial class deletedGameLogicServiceUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameLogicServerUrl",
                table: "GameInfos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameLogicServerUrl",
                table: "GameInfos",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
