using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Raijin.CombinatoricsService.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class DbModelReplacedWithEventStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BooleanProblems");

            migrationBuilder.DropTable(
                name: "DecisionVariableModel");

            migrationBuilder.DropTable(
                name: "CombinatoricProblems");

            migrationBuilder.CreateTable(
                name: "StoredEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StreamId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    EventData = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoredEvents_StreamId",
                table: "StoredEvents",
                column: "StreamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoredEvents");

            migrationBuilder.CreateTable(
                name: "BooleanProblems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Formula = table.Column<string>(type: "text", nullable: false),
                    Satisfiability = table.Column<string>(type: "text", nullable: false),
                    Solution = table.Column<Dictionary<string, bool>>(type: "jsonb", nullable: false)
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
                    Solution = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombinatoricProblems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DecisionVariableModel",
                columns: table => new
                {
                    CombinatoricProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
    }
}
