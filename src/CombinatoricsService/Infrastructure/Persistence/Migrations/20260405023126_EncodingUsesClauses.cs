using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EncodingUsesClauses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dimacs",
                table: "SatEncodings");

            migrationBuilder.CreateTable(
                name: "Clauses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SatEncodingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Literals = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clauses_SatEncodings_SatEncodingId",
                        column: x => x.SatEncodingId,
                        principalTable: "SatEncodings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clauses_SatEncodingId",
                table: "Clauses",
                column: "SatEncodingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clauses");

            migrationBuilder.AddColumn<string>(
                name: "Dimacs",
                table: "SatEncodings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
