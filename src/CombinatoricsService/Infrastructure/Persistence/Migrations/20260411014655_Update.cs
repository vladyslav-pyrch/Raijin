using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SatEncodings_Problems_ProblemId",
                table: "SatEncodings");

            migrationBuilder.DropColumn(
                name: "VariableMap",
                table: "SatEncodings");

            migrationBuilder.AddColumn<Guid>(
                name: "SatRunId",
                table: "Problems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Problems_SatRunId",
                table: "Problems",
                column: "SatRunId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Problems_SatRuns_SatRunId",
                table: "Problems",
                column: "SatRunId",
                principalTable: "SatRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SatEncodings_SatRuns_ProblemId",
                table: "SatEncodings",
                column: "ProblemId",
                principalTable: "SatRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Problems_SatRuns_SatRunId",
                table: "Problems");

            migrationBuilder.DropForeignKey(
                name: "FK_SatEncodings_SatRuns_ProblemId",
                table: "SatEncodings");

            migrationBuilder.DropIndex(
                name: "IX_Problems_SatRunId",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "SatRunId",
                table: "Problems");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "VariableMap",
                table: "SatEncodings",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddForeignKey(
                name: "FK_SatEncodings_Problems_ProblemId",
                table: "SatEncodings",
                column: "ProblemId",
                principalTable: "Problems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
