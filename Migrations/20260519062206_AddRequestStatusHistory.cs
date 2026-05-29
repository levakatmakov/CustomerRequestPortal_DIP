using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerRequestPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    OldStatus = table.Column<string>(type: "TEXT", nullable: false),
                    NewStatus = table.Column<string>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestStatusHistories_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RequestStatusHistories_CustomerRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "CustomerRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestStatusHistories_ChangedByUserId",
                table: "RequestStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestStatusHistories_RequestId",
                table: "RequestStatusHistories",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestStatusHistories");
        }
    }
}
