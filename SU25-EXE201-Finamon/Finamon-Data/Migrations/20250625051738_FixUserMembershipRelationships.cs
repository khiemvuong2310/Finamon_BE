using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finamon_Data.Migrations
{
    /// <inheritdoc />
    public partial class FixUserMembershipRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMemberships_Memberships_MembershipId",
                table: "UserMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMemberships_Memberships_MembershipId1",
                table: "UserMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMemberships_Users_UserId1",
                table: "UserMemberships");

            migrationBuilder.DropIndex(
                name: "IX_UserMemberships_MembershipId1",
                table: "UserMemberships");

            migrationBuilder.DropIndex(
                name: "IX_UserMemberships_UserId1",
                table: "UserMemberships");

            migrationBuilder.DropColumn(
                name: "MembershipId1",
                table: "UserMemberships");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserMemberships");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMemberships_Memberships_MembershipId",
                table: "UserMemberships",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMemberships_Memberships_MembershipId",
                table: "UserMemberships");

            migrationBuilder.AddColumn<int>(
                name: "MembershipId1",
                table: "UserMemberships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserMemberships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserMemberships_MembershipId1",
                table: "UserMemberships",
                column: "MembershipId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserMemberships_UserId1",
                table: "UserMemberships",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMemberships_Memberships_MembershipId",
                table: "UserMemberships",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMemberships_Memberships_MembershipId1",
                table: "UserMemberships",
                column: "MembershipId1",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMemberships_Users_UserId1",
                table: "UserMemberships",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
