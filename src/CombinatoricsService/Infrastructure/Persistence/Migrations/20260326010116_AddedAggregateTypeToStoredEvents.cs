using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedAggregateTypeToStoredEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AggregateType",
                table: "StoredEvents",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AggregateType",
                table: "StoredEvents");
        }
    }
}
