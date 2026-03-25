using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovedNullableSolutions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Dictionary<string, string>>(
                name: "Solution",
                table: "CombinatoricProblems",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, string>),
                oldType: "jsonb",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Dictionary<string, string>>(
                name: "Solution",
                table: "CombinatoricProblems",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(Dictionary<string, string>),
                oldType: "jsonb");
        }
    }
}
