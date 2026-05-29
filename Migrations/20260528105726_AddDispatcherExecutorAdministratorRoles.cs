using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerRequestPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddDispatcherExecutorAdministratorRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedExecutorId",
                table: "CustomerRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "CustomerRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffPosition",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRequests_AssignedExecutorId",
                table: "CustomerRequests",
                column: "AssignedExecutorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerRequests_AspNetUsers_AssignedExecutorId",
                table: "CustomerRequests",
                column: "AssignedExecutorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerRequests_AspNetUsers_AssignedExecutorId",
                table: "CustomerRequests");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRequests_AssignedExecutorId",
                table: "CustomerRequests");

            migrationBuilder.DropColumn(
                name: "AssignedExecutorId",
                table: "CustomerRequests");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "CustomerRequests");

            migrationBuilder.DropColumn(
                name: "StaffPosition",
                table: "AspNetUsers");
        }
    }
}
