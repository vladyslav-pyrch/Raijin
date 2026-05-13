using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class storing_sat_encodign_as_dimacs_string : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clauses");

            migrationBuilder.AddColumn<string>(
                name: "DimacsEncoding",
                table: "Problems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DimacsEncoding",
                table: "Problems");

            migrationBuilder.CreateTable(
                name: "Clauses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Literals = table.Column<int[]>(type: "integer[]", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clauses_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clauses_ProblemId",
                table: "Clauses",
                column: "ProblemId");
        }
    }
}
