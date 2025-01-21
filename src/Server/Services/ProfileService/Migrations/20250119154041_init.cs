using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfileService.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SmallImageUrl = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    BigImageUrl = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PublicName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IsPrivateProfile = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DOB = table.Column<DateOnly>(type: "date", nullable: true),
                    ImageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profiles_ProfileImages_ImageId",
                        column: x => x.ImageId,
                        principalTable: "ProfileImages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileImages_Id",
                table: "ProfileImages",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_ImageId",
                table: "Profiles",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Username",
                table: "Profiles",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "ProfileImages");
        }
    }
}
