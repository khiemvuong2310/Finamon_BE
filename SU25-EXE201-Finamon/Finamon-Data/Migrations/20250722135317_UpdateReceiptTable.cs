using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finamon_Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReceiptTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Receipts",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Receipts");
        }
    }
}
