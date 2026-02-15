using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raijin.SatSolver.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SatProblems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Dimacs = table.Column<string>(type: "text", nullable: false),
                    Solution = table.Column<int[]>(type: "integer[]", nullable: false),
                    Satisfiability = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatProblems", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SatProblems");
        }
    }
}
