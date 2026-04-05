using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Raijin.SatSolver.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Jobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dimacs",
                table: "SatProblems");

            migrationBuilder.AddColumn<string>(
                name: "SolvingStatus",
                table: "SatProblems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ClauseModel",
                columns: table => new
                {
                    SatProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Literals = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClauseModel", x => new { x.SatProblemId, x.Id });
                    table.ForeignKey(
                        name: "FK_ClauseModel_SatProblems_SatProblemId",
                        column: x => x.SatProblemId,
                        principalTable: "SatProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClauseModel");

            migrationBuilder.DropColumn(
                name: "SolvingStatus",
                table: "SatProblems");

            migrationBuilder.AddColumn<string>(
                name: "Dimacs",
                table: "SatProblems",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
