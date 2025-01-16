using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RatingService.Migrations
{
    /// <inheritdoc />
    public partial class updatedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserScores_Seasons_SeasonId",
                table: "UserScores");

            migrationBuilder.RenameColumn(
                name: "UserScoreId",
                table: "UserScores",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SeasonId",
                table: "Seasons",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserScores",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateStart",
                table: "Seasons",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateEnd",
                table: "Seasons",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_DateEnd",
                table: "Seasons",
                column: "DateEnd",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_DateStart",
                table: "Seasons",
                column: "DateStart",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserScores_Seasons_SeasonId",
                table: "UserScores",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserScores_Seasons_SeasonId",
                table: "UserScores");

            migrationBuilder.DropIndex(
                name: "IX_Seasons_DateEnd",
                table: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_Seasons_DateStart",
                table: "Seasons");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserScores",
                newName: "UserScoreId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Seasons",
                newName: "SeasonId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserScores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateStart",
                table: "Seasons",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateEnd",
                table: "Seasons",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddForeignKey(
                name: "FK_UserScores_Seasons_SeasonId",
                table: "UserScores",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "SeasonId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
