using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovedSatEncodingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clauses_SatEncodings_SatEncodingId",
                table: "Clauses");

            migrationBuilder.DropTable(
                name: "SatEncodings");

            migrationBuilder.RenameColumn(
                name: "SatEncodingId",
                table: "Clauses",
                newName: "ProblemId");

            migrationBuilder.RenameIndex(
                name: "IX_Clauses_SatEncodingId",
                table: "Clauses",
                newName: "IX_Clauses_ProblemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clauses_Problems_ProblemId",
                table: "Clauses",
                column: "ProblemId",
                principalTable: "Problems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clauses_Problems_ProblemId",
                table: "Clauses");

            migrationBuilder.RenameColumn(
                name: "ProblemId",
                table: "Clauses",
                newName: "SatEncodingId");

            migrationBuilder.RenameIndex(
                name: "IX_Clauses_ProblemId",
                table: "Clauses",
                newName: "IX_Clauses_SatEncodingId");

            migrationBuilder.CreateTable(
                name: "SatEncodings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatEncodings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatEncodings_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SatEncodings_ProblemId",
                table: "SatEncodings",
                column: "ProblemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clauses_SatEncodings_SatEncodingId",
                table: "Clauses",
                column: "SatEncodingId",
                principalTable: "SatEncodings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
