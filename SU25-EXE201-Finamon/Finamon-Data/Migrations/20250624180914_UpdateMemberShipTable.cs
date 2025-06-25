using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finamon_Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMemberShipTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Memberships");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Memberships",
                newName: "YearlyPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPrice",
                table: "Memberships",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyPrice",
                table: "Memberships");

            migrationBuilder.RenameColumn(
                name: "YearlyPrice",
                table: "Memberships",
                newName: "Price");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Memberships",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
