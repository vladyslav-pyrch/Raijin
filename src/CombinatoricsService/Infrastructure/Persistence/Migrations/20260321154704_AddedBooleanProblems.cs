using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedBooleanProblems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BooleanProblems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Formula = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BooleanProblems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CombinatoricProblems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Constraints = table.Column<string[]>(type: "text[]", nullable: false),
                    Satisfiability = table.Column<string>(type: "text", nullable: false),
                    Solution = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombinatoricProblems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DecisionVariableModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CombinatoricProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    States = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionVariableModel", x => new { x.CombinatoricProblemId, x.Id });
                    table.ForeignKey(
                        name: "FK_DecisionVariableModel_CombinatoricProblems_CombinatoricProb~",
                        column: x => x.CombinatoricProblemId,
                        principalTable: "CombinatoricProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BooleanProblems");

            migrationBuilder.DropTable(
                name: "DecisionVariableModel");

            migrationBuilder.DropTable(
                name: "CombinatoricProblems");
        }
    }
}
