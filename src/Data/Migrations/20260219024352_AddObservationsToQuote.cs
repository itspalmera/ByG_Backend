using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByG_Backend.src.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddObservationsToQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observations",
                table: "Quotes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observations",
                table: "Quotes");
        }
    }
}
