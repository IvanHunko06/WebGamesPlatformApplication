using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchHistoryService.Migrations
{
    /// <inheritdoc />
    public partial class updatedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerScores");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TimeEnd",
                table: "MatchInformations",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TimeBegin",
                table: "MatchInformations",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "GameId",
                table: "MatchInformations",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FinishReason",
                table: "MatchInformations",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "MatchInformations",
                type: "uniqueidentifier",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "UserScoreDeltas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ScoreDelta = table.Column<int>(type: "int", nullable: false),
                    MatchInfoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserScoreDeltas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserScoreDeltas_MatchInformations_MatchInfoId",
                        column: x => x.MatchInfoId,
                        principalTable: "MatchInformations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchInformations_RecordId",
                table: "MatchInformations",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserScoreDeltas_MatchInfoId",
                table: "UserScoreDeltas",
                column: "MatchInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserScoreDeltas");

            migrationBuilder.DropIndex(
                name: "IX_MatchInformations_RecordId",
                table: "MatchInformations");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "MatchInformations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeEnd",
                table: "MatchInformations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeBegin",
                table: "MatchInformations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "GameId",
                table: "MatchInformations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldUnicode: false,
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "FinishReason",
                table: "MatchInformations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.CreateTable(
                name: "PlayerScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchInfoId = table.Column<int>(type: "int", nullable: false),
                    IsWinner = table.Column<bool>(type: "bit", nullable: false),
                    ScorePoints = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerScores_MatchInformations_MatchInfoId",
                        column: x => x.MatchInfoId,
                        principalTable: "MatchInformations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerScores_MatchInfoId",
                table: "PlayerScores",
                column: "MatchInfoId");
        }
    }
}
