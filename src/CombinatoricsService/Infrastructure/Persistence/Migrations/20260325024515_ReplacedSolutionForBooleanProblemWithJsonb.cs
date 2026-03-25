using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplacedSolutionForBooleanProblemWithJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VariableAssignmentModel");

            migrationBuilder.AddColumn<Dictionary<string, bool>>(
                name: "Solution",
                table: "BooleanProblems",
                type: "jsonb",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Solution",
                table: "BooleanProblems");

            migrationBuilder.CreateTable(
                name: "VariableAssignmentModel",
                columns: table => new
                {
                    BooleanProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<bool>(type: "boolean", nullable: false),
                    VariableName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariableAssignmentModel", x => new { x.BooleanProblemId, x.Id });
                    table.ForeignKey(
                        name: "FK_VariableAssignmentModel_BooleanProblems_BooleanProblemId",
                        column: x => x.BooleanProblemId,
                        principalTable: "BooleanProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
